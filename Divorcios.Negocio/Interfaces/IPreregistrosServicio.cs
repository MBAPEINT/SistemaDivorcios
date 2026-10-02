using Divorcios.Negocio.DTOs.Comun;
using Divorcios.Negocio.DTOs.Preregistro;
using Divorcios.Negocio.Resultados;

namespace Divorcios.Negocio.Interfaces
{
    public interface IPreregistrosServicio
    {
        Task<PreregistroDetalleDto> CrearAsync(ActorCiudadano actor, CrearPreregistroDto datos, CancellationToken cancellationToken);
        Task<PaginaDto<PreregistroResumenDto>> ListarAsync(ActorCiudadano actor, ListarPreregistrosDto filtro, CancellationToken cancellationToken);
        Task<PreregistroDetalleDto> ObtenerAsync(ActorCiudadano actor, string preregistroId, CancellationToken cancellationToken);
    }
}
