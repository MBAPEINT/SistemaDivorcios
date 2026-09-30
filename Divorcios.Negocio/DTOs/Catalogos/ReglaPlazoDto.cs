namespace Divorcios.Negocio.DTOs.Catalogos
{
    public sealed record ReglaPlazoDto(
        short ReglaPlazoId,
        string Codigo,
        string Nombre,
        string? Descripcion,
        short Cantidad,
        string UnidadCodigo,
        string TipoDiaCodigo,
        string EventoInicioCodigo,
        DateOnly VigenteDesde,
        DateOnly? VigenteHasta,
        string Fuente,
        bool Activo);
}
