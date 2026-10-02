namespace Divorcios.Negocio.Resultados
{
    // La API construye este actor con las claims de una sesión validada; nunca desde el body.
    public sealed record ActorCiudadano(long CuentaCiudadanaId, long PersonaId, Guid CorrelacionId,
        string? DireccionIp = null, string? UserAgent = null);
}
