using Divorcios.Negocio.DTOs.Preregistro;
using Divorcios.Negocio.Resultados;

namespace Divorcios.Negocio.Interfaces
{
    public interface IVersionesPreregistroServicio
    {
        Task<PreregistroVersionDto> CrearAsync(ActorCiudadano actor, string preregistroId, CrearVersionPreregistroDto datos, CancellationToken cancellationToken);
        Task<IReadOnlyList<PreregistroVersionResumenDto>> ListarAsync(ActorCiudadano actor, string preregistroId, CancellationToken cancellationToken);
        Task<PreregistroVersionDto> ObtenerAsync(ActorCiudadano actor, string preregistroId, string versionId, CancellationToken cancellationToken);
        Task<RequisitosVersionPreregistroDto> ObtenerRequisitosAsync(ActorCiudadano actor, string preregistroId, string versionId, CancellationToken cancellationToken);
    }
}
