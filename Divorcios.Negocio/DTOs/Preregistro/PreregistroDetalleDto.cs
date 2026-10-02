namespace Divorcios.Negocio.DTOs.Preregistro
{
    public sealed record ConyugePreregistroDto(string ExpedienteConyugeId, string PersonaId,
        string PosicionCodigo, bool EsIniciador, string Nombres, string ApellidoPaterno, string ApellidoMaterno);

    public sealed record PreregistroResumenDto(string PreregistroId, string ExpedienteId, string CodigoPreregistro,
        string? NumeroExpediente, string EstadoCodigo, DateTime CreadoEn, DateTime? EnviadoEn,
        bool EstaBloqueado, bool SoyIniciador, IReadOnlyList<ConyugePreregistroDto> Conyuges);

    public sealed record PreregistroDetalleDto(string PreregistroId, string ExpedienteId, string CodigoPreregistro,
        string? NumeroExpediente, string EstadoCodigo, DateTime CreadoEn, DateTime? EnviadoEn,
        DateTime? AprobadoEn, DateTime? BloqueadoEn, bool EstaBloqueado, bool SoyIniciador,
        IReadOnlyList<ConyugePreregistroDto> Conyuges, string? VersionTrabajoId, string? VersionEnviadaId,
        string? VersionAprobadaId, string? RevisionAbiertaId, IReadOnlyList<string> AccionesDisponibles,
        IReadOnlyList<string> AccionesPendientesImplementacion, IReadOnlyList<string> ReferenciasPendientes);
}
