using Divorcios.Datos.Resultados;

namespace Divorcios.Datos.Interfaces
{
    public interface IAlmacenamientoArchivos
    {
        Task<ArchivoAlmacenado> GuardarNuevoAsync(Stream contenido, long maximoBytes, string mimeEsperado, CancellationToken cancellationToken);
        Task<Stream> AbrirVerificadoAsync(string clave, string sha256, long tamanoBytes, string mimeType, CancellationToken cancellationToken);
        // Sólo compensación de una carga nueva cuya falta de referencia se haya comprobado en la BD.
        Task DescartarNuevoAsync(string clave, CancellationToken cancellationToken);
    }
}
