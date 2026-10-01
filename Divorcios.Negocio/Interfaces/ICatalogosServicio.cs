using Divorcios.Negocio.DTOs.Catalogos;

namespace Divorcios.Negocio.Interfaces
{
    public interface ICatalogosServicio
    {
        Task<IReadOnlyList<EstadoExpedienteDto>> ListarEstadosExpedienteAsync(
            bool? activo, CancellationToken cancellationToken);
        Task<EstadoExpedienteDto?> ObtenerEstadoExpedienteAsync(
            int id, CancellationToken cancellationToken);

        Task<IReadOnlyList<TipoDocumentoDto>> ListarTiposDocumentoAsync(
            bool? activo, CancellationToken cancellationToken);
        Task<TipoDocumentoDto?> ObtenerTipoDocumentoAsync(
            int id, CancellationToken cancellationToken);
        Task<TipoDocumentoDto> CrearTipoDocumentoAsync(
            CrearTipoDocumentoDto datos, CancellationToken cancellationToken);

        Task<IReadOnlyList<RequisitoCatalogoDto>> ListarRequisitosAsync(
            bool? activo, CancellationToken cancellationToken);
        Task<RequisitoCatalogoDto?> ObtenerRequisitoCatalogoAsync(
            int id, CancellationToken cancellationToken);

        Task<IReadOnlyList<DestinoOficioDto>> ListarDestinosOficioAsync(
            bool? activo, CancellationToken cancellationToken);
        Task<DestinoOficioDto?> ObtenerDestinoOficioAsync(
            int id, CancellationToken cancellationToken);

        Task<IReadOnlyList<ReglaPlazoDto>> ListarReglasPlazoAsync(
            bool? activo, CancellationToken cancellationToken);
        Task<ReglaPlazoDto?> ObtenerReglaPlazoAsync(
            int id, CancellationToken cancellationToken);

        Task<IReadOnlyList<DiaNoLaborableDto>> ListarDiasNoLaborablesAsync(
            bool? activo, CancellationToken cancellationToken);
        Task<DiaNoLaborableDto?> ObtenerDiaNoLaborableAsync(
            int id, CancellationToken cancellationToken);
    }
}
