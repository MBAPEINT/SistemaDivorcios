using Divorcios.Dominio.Entidades;

namespace Divorcios.Datos.Interfaces
{
    public interface IGeneracionRequisitosRepositorio
    {
        Task<IReadOnlyList<RequisitoCatalogo>> ObtenerCatalogosAsync(IReadOnlyList<string> codigos, CancellationToken cancellationToken);
        Task<IReadOnlyList<TipoDocumento>> ObtenerTiposAsync(IReadOnlyList<string> codigos, CancellationToken cancellationToken);
        Task AgregarAsync(IReadOnlyList<PreregistroRequisito> requisitos, CancellationToken cancellationToken);
    }
}
