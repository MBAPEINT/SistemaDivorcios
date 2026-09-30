namespace Divorcios.Negocio.DTOs.Catalogos
{
    public sealed record EstadoExpedienteDto(
        short EstadoExpedienteId,
        string Codigo,
        string NombreCiudadano,
        string EtapaCodigo,
        short OrdenVisual,
        bool EsFinal,
        bool Activo);
}
