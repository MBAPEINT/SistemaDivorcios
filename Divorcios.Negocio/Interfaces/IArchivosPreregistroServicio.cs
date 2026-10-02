using Divorcios.Negocio.DTOs.Comun;
using Divorcios.Negocio.DTOs.Preregistro;
using Divorcios.Negocio.Resultados;

namespace Divorcios.Negocio.Interfaces
{
    public interface IArchivosPreregistroServicio
    {
        Task<DocumentoVersionDto> CargarAsync(ActorCiudadano actor, string preregistroId, string versionId,
            string requisitoId, short tipoDocumentoId, string titulo, ArchivoEntrada archivo, CancellationToken cancellationToken);
        Task<DocumentoVersionDto> CorregirAsync(ActorCiudadano actor, string preregistroId, string versionId,
            string requisitoId, string documentoId, string documentoVersionBaseId, string motivoCambio, ArchivoEntrada archivo, CancellationToken cancellationToken);
        Task<PreregistroRequisitoDto> SeleccionarAsync(ActorCiudadano actor, string preregistroId, string versionId,
            string requisitoId, SeleccionarArchivosPreregistroDto datos, CancellationToken cancellationToken);
        Task<PaginaDto<DocumentoPreregistroDto>> ListarAsync(ActorCiudadano actor, string preregistroId,
            ListarDocumentosPreregistroDto filtro, CancellationToken cancellationToken);
        Task<DescargaArchivo> DescargarAsync(ActorCiudadano actor, string preregistroId, string documentoId,
            string documentoVersionId, CancellationToken cancellationToken);
    }
}
