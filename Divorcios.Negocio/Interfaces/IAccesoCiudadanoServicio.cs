using Divorcios.Negocio.DTOs.Identidad;

namespace Divorcios.Negocio.Interfaces
{
    public interface IAccesoCiudadanoServicio
    {
        Task<CiudadanoAutenticadoDto> IniciarSesionAsync(IniciarSesionCiudadanaDto datos, CancellationToken cancellationToken);
        Task<CiudadanoAutenticadoDto?> ObtenerSesionAsync(long cuentaId, CancellationToken cancellationToken);
    }
}
