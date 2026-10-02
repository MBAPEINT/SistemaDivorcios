using Divorcios.Datos.Resultados;
using Divorcios.Dominio.Entidades;

namespace Divorcios.Datos.Interfaces
{
    public interface IVersionesPreregistroRepositorio
    {
        Task<T> EjecutarEscrituraAsync<T>(long preregistroId, Func<Task<T>> operacion, CancellationToken cancellationToken);
        Task<LecturaVersionesPreregistro?> LeerAsync(long preregistroId, long personaId, long? versionId,
            bool incluirRequisitos, CancellationToken cancellationToken);
        Task<IReadOnlyList<ExpedienteContactoHistorial>> ObtenerContactosActualesAsync(long expedienteId, CancellationToken cancellationToken);
        Task AgregarVersionAsync(PreregistroVersion version, CancellationToken cancellationToken);
        Task GuardarAsync(CancellationToken cancellationToken);
        Task AgregarContactosAsync(IEnumerable<ExpedienteContactoHistorial> contactos, CancellationToken cancellationToken);
    }
}
