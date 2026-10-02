using Divorcios.Dominio.Entidades;

namespace Divorcios.Datos.Interfaces
{
    public interface IAccesoCiudadanoRepositorio
    {
        Task<CuentaCiudadana?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken);
        Task<CuentaCiudadana?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken);
        Task<T> EjecutarSerializadoAsync<T>(Func<Task<T>> operacion, CancellationToken cancellationToken);
        Task GuardarAsync(CuentaCiudadana cuenta, CancellationToken cancellationToken);
    }
}
