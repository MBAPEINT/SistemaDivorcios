using Divorcios.Dominio.Entidades;

namespace Divorcios.Datos.Interfaces
{
    public interface IIdentidadRepositorio
    {
        Task<T> EjecutarSerializadoAsync<T>(string dni, Func<Task<T>> operacion, CancellationToken cancellationToken);
        Task<ConsultaReniec?> ObtenerCacheAsync(string dni, DateTime ahora, CancellationToken cancellationToken);
        Task<int> ContarIntentosAsync(DateTime inicio, DateTime fin, CancellationToken cancellationToken);
        Task GuardarAsync(ConsultaReniec consulta, CancellationToken cancellationToken);
    }
}
