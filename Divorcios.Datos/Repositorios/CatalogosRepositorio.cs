using Divorcios.Datos.Contexto;
using Divorcios.Datos.Interfaces;
using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Divorcios.Datos.Repositorios
{
    public sealed class CatalogosRepositorio(DivorciosDbContext contexto) : ICatalogosRepositorio
    {
        public async Task<IReadOnlyList<EstadoExpediente>> ListarEstadosExpedienteAsync(
            bool? activo, CancellationToken cancellationToken)
        {
            return await contexto.EstadosExpediente.AsNoTracking()
                .Where(x => !activo.HasValue || x.Activo == activo.Value)
                .OrderBy(x => x.EtapaCodigo).ThenBy(x => x.OrdenVisual).ThenBy(x => x.EstadoExpedienteId)
                .ToListAsync(cancellationToken);
        }

        public Task<EstadoExpediente?> ObtenerEstadoExpedienteAsync(
            int id, CancellationToken cancellationToken)
        {
            return contexto.EstadosExpediente.AsNoTracking()
                .SingleOrDefaultAsync(x => x.EstadoExpedienteId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<TipoDocumento>> ListarTiposDocumentoAsync(
            bool? activo, CancellationToken cancellationToken)
        {
            return await contexto.TiposDocumento.AsNoTracking()
                .Where(x => !activo.HasValue || x.Activo == activo.Value)
                .OrderBy(x => x.Codigo).ThenBy(x => x.TipoDocumentoId)
                .ToListAsync(cancellationToken);
        }

        public Task<TipoDocumento?> ObtenerTipoDocumentoAsync(
            int id, CancellationToken cancellationToken)
        {
            return contexto.TiposDocumento.AsNoTracking()
                .SingleOrDefaultAsync(x => x.TipoDocumentoId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<RequisitoCatalogo>> ListarRequisitosAsync(
            bool? activo, CancellationToken cancellationToken)
        {
            return await contexto.RequisitosCatalogo.AsNoTracking()
                .Where(x => !activo.HasValue || x.Activo == activo.Value)
                .OrderBy(x => x.Codigo).ThenBy(x => x.RequisitoCatalogoId)
                .ToListAsync(cancellationToken);
        }

        public Task<RequisitoCatalogo?> ObtenerRequisitoCatalogoAsync(
            int id, CancellationToken cancellationToken)
        {
            return contexto.RequisitosCatalogo.AsNoTracking()
                .SingleOrDefaultAsync(x => x.RequisitoCatalogoId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<DestinoOficio>> ListarDestinosOficioAsync(
            bool? activo, CancellationToken cancellationToken)
        {
            return await contexto.DestinosOficio.AsNoTracking()
                .Where(x => !activo.HasValue || x.Activo == activo.Value)
                .OrderBy(x => x.Codigo).ThenBy(x => x.DestinoOficioId)
                .ToListAsync(cancellationToken);
        }

        public Task<DestinoOficio?> ObtenerDestinoOficioAsync(
            int id, CancellationToken cancellationToken)
        {
            return contexto.DestinosOficio.AsNoTracking()
                .SingleOrDefaultAsync(x => x.DestinoOficioId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<ReglaPlazo>> ListarReglasPlazoAsync(
            bool? activo, CancellationToken cancellationToken)
        {
            return await contexto.ReglasPlazo.AsNoTracking()
                .Where(x => !activo.HasValue || x.Activo == activo.Value)
                .OrderBy(x => x.Codigo).ThenByDescending(x => x.VigenteDesde).ThenBy(x => x.ReglaPlazoId)
                .ToListAsync(cancellationToken);
        }

        public Task<ReglaPlazo?> ObtenerReglaPlazoAsync(
            int id, CancellationToken cancellationToken)
        {
            return contexto.ReglasPlazo.AsNoTracking()
                .SingleOrDefaultAsync(x => x.ReglaPlazoId == id, cancellationToken);
        }

        public async Task<IReadOnlyList<DiaNoLaborable>> ListarDiasNoLaborablesAsync(
            bool? activo, CancellationToken cancellationToken)
        {
            return await contexto.DiasNoLaborables.AsNoTracking()
                .Where(x => !activo.HasValue || x.Activo == activo.Value)
                .OrderBy(x => x.Fecha).ThenBy(x => x.DiaNoLaborableId)
                .ToListAsync(cancellationToken);
        }

        public Task<DiaNoLaborable?> ObtenerDiaNoLaborableAsync(
            int id, CancellationToken cancellationToken)
        {
            return contexto.DiasNoLaborables.AsNoTracking()
                .SingleOrDefaultAsync(x => x.DiaNoLaborableId == id, cancellationToken);
        }
    }
}
