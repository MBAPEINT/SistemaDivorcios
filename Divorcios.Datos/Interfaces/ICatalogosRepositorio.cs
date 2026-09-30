using Divorcios.Dominio.Entidades;

namespace Divorcios.Datos.Interfaces
{
    public interface ICatalogosRepositorio
    {
        Task<IReadOnlyList<EstadoExpediente>> ListarEstadosExpedienteAsync(
            bool? activo, CancellationToken cancellationToken);
        Task<EstadoExpediente?> ObtenerEstadoExpedienteAsync(
            int id, CancellationToken cancellationToken);

        Task<IReadOnlyList<TipoDocumento>> ListarTiposDocumentoAsync(
            bool? activo, CancellationToken cancellationToken);
        Task<TipoDocumento?> ObtenerTipoDocumentoAsync(
            int id, CancellationToken cancellationToken);

        Task<IReadOnlyList<RequisitoCatalogo>> ListarRequisitosAsync(
            bool? activo, CancellationToken cancellationToken);
        Task<RequisitoCatalogo?> ObtenerRequisitoCatalogoAsync(
            int id, CancellationToken cancellationToken);

        Task<IReadOnlyList<DestinoOficio>> ListarDestinosOficioAsync(
            bool? activo, CancellationToken cancellationToken);
        Task<DestinoOficio?> ObtenerDestinoOficioAsync(
            int id, CancellationToken cancellationToken);

        Task<IReadOnlyList<ReglaPlazo>> ListarReglasPlazoAsync(
            bool? activo, CancellationToken cancellationToken);
        Task<ReglaPlazo?> ObtenerReglaPlazoAsync(
            int id, CancellationToken cancellationToken);

        Task<IReadOnlyList<DiaNoLaborable>> ListarDiasNoLaborablesAsync(
            bool? activo, CancellationToken cancellationToken);
        Task<DiaNoLaborable?> ObtenerDiaNoLaborableAsync(
            int id, CancellationToken cancellationToken);
    }
}
