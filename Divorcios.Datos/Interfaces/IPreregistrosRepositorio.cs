using Divorcios.Datos.Resultados;
using Divorcios.Dominio.Entidades;

namespace Divorcios.Datos.Interfaces
{
    public interface IPreregistrosRepositorio
    {
        Task<T> EjecutarTransaccionAsync<T>(Func<Task<T>> operacion, CancellationToken cancellationToken);
        Task<CuentaCiudadana?> ObtenerCuentaAsync(long cuentaId, CancellationToken cancellationToken);
        Task<IReadOnlyList<Persona>> ObtenerPersonasAsync(IReadOnlyList<long> ids, CancellationToken cancellationToken);
        Task AgregarAsync(Preregistro preregistro, CancellationToken cancellationToken);
        Task RegistrarAuditoriaAsync(RegistroAuditoria registro, CancellationToken cancellationToken);
        Task<CabeceraPreregistro?> ObtenerAsync(long preregistroId, long personaId, CancellationToken cancellationToken);
        Task<PaginaPreregistros> ListarAsync(long personaId, string? estadoCodigo, int pagina, int tamanoPagina, CancellationToken cancellationToken);
    }
}
