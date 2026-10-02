using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Divorcios.Negocio.DTOs.Preregistro
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed class SeleccionarArchivosPreregistroDto
    {
        [Required, MaxLength(100)]
        public List<string> DocumentoVersionIds { get; set; } = null!;
    }
    public sealed class ListarDocumentosPreregistroDto
    {
        [Range(1, 32767)] public short? TipoDocumentoId { get; set; }
        [Range(1, int.MaxValue)] public int Pagina { get; set; } = 1;
        [Range(1, 100)] public int TamanoPagina { get; set; } = 20;
    }
    public sealed record DocumentoVersionDto(string DocumentoId, string DocumentoVersionId, int NumeroVersion,
        short TipoDocumentoId, string Titulo, string NombreArchivo, string MimeType, long TamanoBytes,
        string Sha256, DateTime CreadoEn, string UrlDescarga);
    public sealed record DocumentoPreregistroDto(string DocumentoId, short TipoDocumentoId, string TipoDocumentoCodigo,
        bool TipoActivo, string Titulo, string EstadoCodigo, string? PreregistroRequisitoOrigenId,
        IReadOnlyList<DocumentoVersionListadoDto> Versiones);
    public sealed record DocumentoVersionListadoDto(DocumentoVersionDto Archivo,
        IReadOnlyList<PresentacionArchivoDto> Presentaciones, IReadOnlyList<EvaluacionArchivoDto> Evaluaciones);
    public sealed record PresentacionArchivoDto(string PreregistroVersionId, string PreregistroRequisitoId);
    public sealed record EvaluacionArchivoDto(string RevisionPreregistroId, string RevisionDetalleId, string ResultadoCodigo);
}
