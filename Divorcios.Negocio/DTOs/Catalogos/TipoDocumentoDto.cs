namespace Divorcios.Negocio.DTOs.Catalogos
{
    public sealed record TipoDocumentoDto(
        short TipoDocumentoId,
        string Codigo,
        string Nombre,
        string OrigenCodigo,
        bool Activo);
}
