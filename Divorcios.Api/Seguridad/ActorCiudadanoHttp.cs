using System.Globalization;
using Divorcios.Negocio.Excepciones;
using Divorcios.Negocio.Resultados;

namespace Divorcios.Api.Seguridad
{
    public static class ActorCiudadanoHttp
    {
        public static ActorCiudadano Obtener(HttpContext contexto)
        {
            if (contexto.User.Identity?.IsAuthenticated != true
                || !long.TryParse(contexto.User.FindFirst("sub")?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var cuentaId)
                || !long.TryParse(contexto.User.FindFirst("persona_id")?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var personaId)
                || cuentaId <= 0 || personaId <= 0) throw new AccesoCiudadanoRechazadoException();
            return new(cuentaId, personaId, Guid.NewGuid(), contexto.Connection.RemoteIpAddress?.ToString(),
                contexto.Request.Headers.UserAgent.ToString());
        }
    }
}
