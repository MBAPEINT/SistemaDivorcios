namespace Divorcios.Negocio.DTOs.Preregistro
{
    public sealed record InformacionPreregistroDto(
        string Descripcion, IReadOnlyList<string> Pasos, IReadOnlyList<string> Advertencias,
        IReadOnlyList<FormatoPreregistroDto> Formatos, PoliticaArchivosDto? PoliticaArchivos,
        IReadOnlyList<string> PendientesConfiguracion);
    public sealed record FormatoPreregistroDto(string Codigo, string Nombre, string Url, DateOnly? ActualizadoEn = null);
    public sealed record PoliticaArchivosDto(IReadOnlyList<string> MimePermitidos, long MaximoBytes);
}
