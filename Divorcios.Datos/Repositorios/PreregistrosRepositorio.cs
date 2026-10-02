using System.Data;
using Divorcios.Datos.Contexto;
using Divorcios.Datos.Excepciones;
using Divorcios.Datos.Interfaces;
using Divorcios.Datos.Resultados;
using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Divorcios.Datos.Repositorios
{
    public sealed class PreregistrosRepositorio(DivorciosDbContext contexto) : IPreregistrosRepositorio
    {
        public async Task<T> EjecutarTransaccionAsync<T>(Func<Task<T>> operacion, CancellationToken cancellationToken)
        {
            await using var transaccion = await contexto.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
            try
            {
                var resultado = await operacion();
                await transaccion.CommitAsync(cancellationToken);
                return resultado;
            }
            catch (DbUpdateException excepcion) when (excepcion.InnerException is PostgresException
                { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "ix_expediente_codigo_preregistro" })
            {
                contexto.ChangeTracker.Clear();
                throw new CodigoDuplicadoPersistenciaException("El código digital generado ya está registrado.", excepcion);
            }
        }

        public Task<CuentaCiudadana?> ObtenerCuentaAsync(long cuentaId, CancellationToken cancellationToken)
            => contexto.CuentasCiudadanas.AsNoTracking().SingleOrDefaultAsync(x => x.CuentaCiudadanaId == cuentaId, cancellationToken);

        public async Task<IReadOnlyList<Persona>> ObtenerPersonasAsync(IReadOnlyList<long> ids, CancellationToken cancellationToken)
            => await contexto.Personas.AsNoTracking().Where(x => ids.Contains(x.PersonaId)).ToListAsync(cancellationToken);

        public async Task AgregarAsync(Preregistro preregistro, CancellationToken cancellationToken)
        {
            contexto.Preregistros.Add(preregistro);
            await contexto.SaveChangesAsync(cancellationToken);
        }

        public async Task RegistrarAuditoriaAsync(RegistroAuditoria registro, CancellationToken cancellationToken)
        {
            contexto.RegistrosAuditoria.Add(registro);
            await contexto.SaveChangesAsync(cancellationToken);
        }

        private IQueryable<Preregistro> Visibles(long personaId) => contexto.Preregistros.AsNoTracking()
            .Where(x => x.Expediente.Conyuges.Any(c => c.PersonaId == personaId))
            .Include(x => x.Expediente).ThenInclude(x => x.Conyuges).ThenInclude(x => x.Persona);

        public async Task<CabeceraPreregistro?> ObtenerAsync(long preregistroId, long personaId, CancellationToken cancellationToken)
        {
            if (contexto.Database.CurrentTransaction is not null)
                return await LeerCabeceraAsync(preregistroId, personaId, cancellationToken);
            await using var transaccion = await contexto.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
            var resultado = await LeerCabeceraAsync(preregistroId, personaId, cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
            return resultado;
        }

        private async Task<CabeceraPreregistro?> LeerCabeceraAsync(long preregistroId, long personaId, CancellationToken cancellationToken)
        {
            var registro = await Visibles(personaId).SingleOrDefaultAsync(x => x.PreregistroId == preregistroId, cancellationToken);
            if (registro is null) return null;
            var versiones = await contexto.PreregistrosVersiones.AsNoTracking()
                .Where(x => x.PreregistroId == preregistroId).OrderByDescending(x => x.NumeroVersion)
                .Select(x => new ReferenciaVersionPreregistro(x.PreregistroVersionId, x.NumeroVersion, x.CreadoEn)).ToListAsync(cancellationToken);
            var revisiones = await contexto.RevisionesPrerregistro.AsNoTracking()
                .Where(x => x.PreregistroVersion.PreregistroId == preregistroId)
                .Select(x => new ReferenciaRevisionPreregistro(x.RevisionPreregistroId, x.PreregistroVersionId,
                    x.ResultadoCodigo, x.IniciadaEn, x.FinalizadaEn)).ToListAsync(cancellationToken);
            string? enviada = null;
            if (registro.EnviadoEn is not null)
                enviada = await contexto.RegistrosAuditoria.AsNoTracking()
                    .Where(x => x.ExpedienteId == registro.ExpedienteId && x.AccionCodigo == "PREREGISTRO_ENVIADO"
                        && x.RecursoCodigo == "PREREGISTRO_VERSION" && x.ResultadoCodigo == "EXITO"
                        && x.RegistradoEn == registro.EnviadoEn)
                    .OrderByDescending(x => x.RegistroAuditoriaId).Select(x => x.RecursoId).FirstOrDefaultAsync(cancellationToken);
            return new(registro, versiones, revisiones, enviada);
        }

        public async Task<PaginaPreregistros> ListarAsync(long personaId, string? estadoCodigo, int pagina, int tamanoPagina, CancellationToken cancellationToken)
        {
            var consulta = Visibles(personaId).Where(x => estadoCodigo == null || x.EstadoCodigo == estadoCodigo);
            // Conteo y página comparten una instantánea para evitar inconsistencias por altas concurrentes.
            await using var transaccion = await contexto.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
            var total = await consulta.LongCountAsync(cancellationToken);
            var desplazamiento = ((long)pagina - 1) * tamanoPagina;
            IReadOnlyList<Preregistro> items = desplazamiento > int.MaxValue ? [] : await consulta
                .OrderByDescending(x => x.CreadoEn).ThenByDescending(x => x.PreregistroId)
                .Skip((int)desplazamiento).Take(tamanoPagina).ToListAsync(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
            return new(items, total);
        }
    }
}
