namespace Divorcios.Negocio.DTOs.Catalogos
{
    public sealed record DiaNoLaborableDto(
        int DiaNoLaborableId,
        DateOnly Fecha,
        string Nombre,
        string TipoCodigo,
        string AmbitoCodigo,
        bool ExcluyeDiaHabil,
        bool ExcluyeDiaOperativo,
        bool Activo);
}
