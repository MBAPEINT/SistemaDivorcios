using Divorcios.Datos.Resultados;
using Divorcios.Dominio.Entidades;

namespace Divorcios.Datos.Interfaces
{
    public interface IArchivosPreregistroRepositorio
    {
        Task<PreregistroRequisito?> ObtenerRequisitoAsync(long versionId, long requisitoId, CancellationToken cancellationToken);
        Task<TipoDocumento?> ObtenerTipoAsync(short tipoId, CancellationToken cancellationToken);
        Task<Documento?> ObtenerDocumentoAsync(long expedienteId, long documentoId, CancellationToken cancellationToken);
        Task<IReadOnlyList<DocumentoVersion>> ObtenerVersionesAsync(long expedienteId, IReadOnlyList<long> ids, CancellationToken cancellationToken);
        Task<DocumentoVersion?> ObtenerArchivoAsync(long preregistroId, long personaId, long documentoId, long versionId, CancellationToken cancellationToken);
        Task<PaginaDocumentosPreregistro> ListarAsync(long preregistroId, long personaId, short? tipoId, int pagina, int tamanoPagina, CancellationToken cancellationToken);
        Task AgregarAsync(DocumentoVersion version, CancellationToken cancellationToken);
        Task SeleccionarAsync(PreregistroRequisito requisito, IReadOnlyList<long> ids, CancellationToken cancellationToken);
        Task<bool> EstaClavePersistidaAsync(string clave, CancellationToken cancellationToken);
    }
}
