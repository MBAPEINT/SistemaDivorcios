using Divorcios.Datos.Contexto;
using Divorcios.Datos.Interfaces;
using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Divorcios.Datos.Repositorios
{
    public sealed class IdentidadRepositorio(DivorciosDbContext contexto) : IIdentidadRepositorio
    {
        public async Task<T> EjecutarSerializadoAsync<T>(string dni, Func<Task<T>> operacion, CancellationToken cancellationToken)
        {
            await using var transaccion = await contexto.Database.BeginTransactionAsync(cancellationToken);
            // Bloqueo entre procesos: protege la cuota y evita dos consultas concurrentes del mismo DNI.
            // La llamada externa queda serializada; no hay reintentos automáticos que consuman cuota.
            await contexto.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(734021, 1)", cancellationToken);
            var resultado = await operacion();
            // Una vez realizado el intento externo, persistirlo aunque el cliente se desconecte.
            await transaccion.CommitAsync(CancellationToken.None);
            return resultado;
        }

        public Task<ConsultaReniec?> ObtenerCacheAsync(string dni, DateTime ahora, CancellationToken cancellationToken)
            => contexto.ConsultasReniec.AsNoTracking()
                .Where(x => x.DniConsultado == dni && x.ResultadoCodigo == "ENCONTRADO"
                    && x.PersonaId != null && (x.ExpiraEn == null || x.ExpiraEn > ahora))
                .OrderByDescending(x => x.ConsultadoEn).ThenByDescending(x => x.ConsultaReniecId)
                .FirstOrDefaultAsync(cancellationToken);

        public Task<int> ContarIntentosAsync(DateTime inicio, DateTime fin, CancellationToken cancellationToken)
            => contexto.ConsultasReniec.CountAsync(x => x.OrigenCodigo == "API"
                && x.ConsultadoEn >= inicio && x.ConsultadoEn <= fin, cancellationToken);

        public async Task GuardarAsync(ConsultaReniec consulta, CancellationToken cancellationToken)
        {
            if (consulta.ResultadoCodigo == "ENCONTRADO")
            {
                var persona = await contexto.Personas.SingleOrDefaultAsync(x => x.Dni == consulta.DniConsultado, cancellationToken);
                if (persona is null)
                {
                    persona = new Persona { Dni = consulta.DniConsultado, CreadoEn = consulta.ConsultadoEn };
                    contexto.Personas.Add(persona);
                }
                persona.Nombres = consulta.Prenombres!;
                persona.ApellidoPaterno = consulta.ApellidoPaterno!;
                persona.ApellidoMaterno = consulta.ApellidoMaterno!;
                persona.DireccionDni = consulta.Direccion;
                persona.VerificadoReniec = true;
                persona.VerificadoReniecEn = consulta.ConsultadoEn;
                persona.ActualizadoEn = consulta.ConsultadoEn;
                consulta.Persona = persona;
            }
            if (consulta.ConsultaReniecId == 0) contexto.ConsultasReniec.Add(consulta);
            await contexto.SaveChangesAsync(cancellationToken);
        }
    }
}
