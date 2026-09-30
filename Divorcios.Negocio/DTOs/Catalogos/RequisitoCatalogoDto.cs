namespace Divorcios.Negocio.DTOs.Catalogos
{
    public sealed record RequisitoCatalogoDto(
        short RequisitoCatalogoId,
        string Codigo,
        string Nombre,
        string? Descripcion,
        bool Activo);
}
