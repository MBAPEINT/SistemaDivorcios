namespace Divorcios.Negocio.DTOs.Identidad
{
    public sealed record ConsultaIdentidadDto(
        string ConsultaReniecId, string ResultadoCodigo, string? PersonaId,
        string? Nombres, string? ApellidoPaterno, string? ApellidoMaterno,
        DateTime ConsultadoEn, bool DesdeCache);
}
