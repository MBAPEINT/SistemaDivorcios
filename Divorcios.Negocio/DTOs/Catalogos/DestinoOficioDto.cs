namespace Divorcios.Negocio.DTOs.Catalogos
{
    public sealed record DestinoOficioDto(
        short DestinoOficioId,
        string Codigo,
        string Nombre,
        bool Activo);
}
