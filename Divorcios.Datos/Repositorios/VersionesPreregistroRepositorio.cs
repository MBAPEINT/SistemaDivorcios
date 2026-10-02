using System.Data;
using Divorcios.Datos.Contexto;
using Divorcios.Datos.Interfaces;
using Divorcios.Datos.Resultados;
using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Divorcios.Datos.Repositorios
{
    public sealed class VersionesPreregistroRepositorio(DivorciosDbContext contexto, IPreregistrosRepositorio preregistros)
        : IVersionesPreregistroRepositorio
    {
        public async Task<T> EjecutarEscrituraAsync<T>(long preregistroId, Func<Task<T>> operacion, CancellationToken cancellationToken)
        {
            // READ COMMITTED: después de esperar el bloqueo, la precondición ve el último guardado confirmado.
            await using var transaccion = await contexto.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            try
            {
                await contexto.Database.SqlQuery<long>($"""
                    SELECT e.expediente_id AS "Value"
                    FROM divorcios.expediente e
                    JOIN divorcios.preregistro p ON p.expediente_id = e.expediente_id
                    WHERE p.preregistro_id = {preregistroId}
                    FOR UPDATE OF e, p
                    """).ToListAsync(cancellationToken);
                var resultado = await operacion();
                await transaccion.CommitAsync(cancellationToken);
                return resultado;
            }
            catch
            {
                contexto.ChangeTracker.Clear();
                throw;
            }
        }

        public async Task<LecturaVersionesPreregistro?> LeerAsync(long preregistroId, long personaId,
            long? versionId, bool incluirRequisitos, CancellationToken cancellationToken)
        {
            if (contexto.Database.CurrentTransaction is not null)
                return await LeerInstantaneaAsync(preregistroId, personaId, versionId, incluirRequisitos, cancellationToken);
            await using var transaccion = await contexto.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
            var resultado = await LeerInstantaneaAsync(preregistroId, personaId, versionId, incluirRequisitos, cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
            return resultado;
        }

        private async Task<LecturaVersionesPreregistro?> LeerInstantaneaAsync(long preregistroId, long personaId,
            long? versionId, bool incluirRequisitos, CancellationToken cancellationToken)
        {
            var cabecera = await preregistros.ObtenerAsync(preregistroId, personaId, cancellationToken);
            if (cabecera is null) return null;
            var consulta = contexto.PreregistrosVersiones.AsNoTracking()
                .Where(x => x.PreregistroId == preregistroId && (versionId == null || x.PreregistroVersionId == versionId));
            if (incluirRequisitos)
                consulta = consulta.Include(x => x.Requisitos).ThenInclude(x => x.RequisitoCatalogo)
                    .Include(x => x.Requisitos).ThenInclude(x => x.ArchivosPresentados)
                    .ThenInclude(x => x.DocumentoVersion).ThenInclude(x => x.Documento);
            var versiones = await consulta.OrderByDescending(x => x.NumeroVersion).ToListAsync(cancellationToken);
            if (versionId is not null && versiones.Count == 0) return null;
            IReadOnlyList<ExpedienteContactoHistorial> contactos = [];
            if (versionId is not null)
            {
                var fecha = versiones[0].CreadoEn;
                contactos = await contexto.ExpedientesContactosHistorial.AsNoTracking()
                    .Where(x => x.ExpedienteConyuge.ExpedienteId == cabecera.Registro.ExpedienteId
                        && x.VigenteDesde <= fecha && (x.VigenteHasta == null || x.VigenteHasta > fecha))
                    .ToListAsync(cancellationToken);
            }
            var eventos = await contexto.RegistrosAuditoria.AsNoTracking()
                .Where(x => x.ExpedienteId == cabecera.Registro.ExpedienteId && x.RecursoCodigo == "PREREGISTRO_VERSION"
                    && x.ResultadoCodigo == "EXITO"
                    && (x.AccionCodigo == "PREREGISTRO_VERSION_CREADA" || x.AccionCodigo == "PREREGISTRO_ENVIADO"))
                .ToListAsync(cancellationToken);
            return new(cabecera, versiones, contactos, eventos);
        }

        public async Task<IReadOnlyList<ExpedienteContactoHistorial>> ObtenerContactosActualesAsync(long expedienteId, CancellationToken cancellationToken)
            => await contexto.ExpedientesContactosHistorial
                .Where(x => x.ExpedienteConyuge.ExpedienteId == expedienteId && x.VigenteHasta == null)
                .ToListAsync(cancellationToken);

        public async Task AgregarVersionAsync(PreregistroVersion version, CancellationToken cancellationToken)
        {
            contexto.PreregistrosVersiones.Add(version);
            await contexto.SaveChangesAsync(cancellationToken);
        }

        public async Task GuardarAsync(CancellationToken cancellationToken) => await contexto.SaveChangesAsync(cancellationToken);

        public async Task AgregarContactosAsync(IEnumerable<ExpedienteContactoHistorial> contactos, CancellationToken cancellationToken)
        {
            contexto.ExpedientesContactosHistorial.AddRange(contactos);
            await contexto.SaveChangesAsync(cancellationToken);
        }
    }
}
