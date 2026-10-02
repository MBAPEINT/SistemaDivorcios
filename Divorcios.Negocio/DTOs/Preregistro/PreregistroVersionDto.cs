namespace Divorcios.Negocio.DTOs.Preregistro
{
    public sealed record EncuestaPreregistroDto(DateOnly FechaMatrimonio, bool MatrimonioEnPorvenir,
        bool UltimoDomicilioConyugalPorvenir, string? DomicilioConyugal, bool TieneHijos,
        short CantidadHijosMenores, short CantidadHijosMayores, bool? TieneHijosMayoresSituacionEspecial,
        bool TieneBienes, bool TieneAcuerdoBienes, bool? RequiereRepresentacionA,
        bool? RequiereRepresentacionB, string? ObservacionCiudadano);

    public sealed record PreregistroVersionDto(string PreregistroId, string PreregistroVersionId,
        int NumeroVersion, DateTime CreadoEn, string? MotivoCambio, EncuestaPreregistroDto Encuesta,
        IReadOnlyList<ContactoPreregistroDto> Contactos, bool Editable, string EstadoGeneracionRequisitosCodigo);

    // null = no existe evidencia suficiente de envío; false = el guardado acredita un borrador sin envío.
    public sealed record PreregistroVersionResumenDto(string PreregistroVersionId, int NumeroVersion,
        DateTime CreadoEn, string? MotivoCambio, bool EsVersionTrabajo, bool? Enviada,
        IReadOnlyList<RevisionVersionReferenciaDto> Revisiones, string EstadoGeneracionRequisitosCodigo);
    public sealed record RevisionVersionReferenciaDto(string RevisionPreregistroId, string ResultadoCodigo,
        DateTime IniciadaEn, DateTime? FinalizadaEn);

    public sealed record RequisitosVersionPreregistroDto(string PreregistroVersionId,
        string EstadoGeneracionCodigo, IReadOnlyList<string> PendientesConfiguracion,
        IReadOnlyList<PreregistroRequisitoDto> Items);
    public sealed record PreregistroRequisitoDto(string PreregistroRequisitoId, short RequisitoCatalogoId,
        string Codigo, string Nombre, string? Descripcion, bool Aplica, bool Obligatorio,
        string EstadoCodigo, IReadOnlyList<ArchivoPresentadoPreregistroDto> ArchivosPresentados, DeterminacionRequisitoDto? Determinacion = null);
    public sealed record DeterminacionRequisitoDto(string MatrizVersion, string NombreAlGenerar, string TitularesCodigo,
        int? CantidadTitulares, string CoberturaEsperada, bool PermiteVariosArchivos,
        IReadOnlyList<string> TiposDocumentoCodigos, IReadOnlyList<string> Fuentes);
    public sealed record ArchivoPresentadoPreregistroDto(string DocumentoId, string DocumentoVersionId,
        int NumeroVersion, string NombreArchivo, string MimeType, long TamanoBytes, string Sha256, DateTime CreadoEn);
}
