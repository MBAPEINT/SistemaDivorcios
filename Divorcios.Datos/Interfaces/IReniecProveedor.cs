using Divorcios.Datos.Resultados;

namespace Divorcios.Datos.Interfaces
{
    public interface IReniecProveedor
    {
        Task<RespuestaReniec> ConsultarAsync(string dni, CancellationToken cancellationToken);
    }
}
