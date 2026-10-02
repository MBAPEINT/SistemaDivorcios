namespace Divorcios.Negocio.DTOs.Comun
{
    public sealed record PaginaDto<T>(IReadOnlyList<T> Items, int Pagina, int TamanoPagina, long Total);
}
