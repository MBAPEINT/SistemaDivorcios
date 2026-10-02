using Divorcios.Negocio.DTOs.Identidad;
using Divorcios.Negocio.Resultados;

namespace Divorcios.Negocio.Interfaces
{
    public interface IIdentidadServicio
    {
        Task<ResultadoConsultaIdentidad> ConsultarAsync(ConsultarDniDto datos, CancellationToken cancellationToken);
    }
}
