using System.Globalization;
using Divorcios.Api.Seguridad;
using Divorcios.Negocio.DTOs.Identidad;
using Divorcios.Negocio.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Divorcios.Api.Controllers
{
    [ApiController, Route("api/acceso-ciudadano"), Tags("Acceso ciudadano provisional")]
    public sealed class AccesoCiudadanoController(IAccesoCiudadanoServicio servicio, EmisorSesionCiudadana emisor) : ControllerBase
    {
        [AllowAnonymous, EnableRateLimiting("AccesoCiudadano"), HttpPost("sesion")]
        [EndpointSummary("Iniciar sesión por DNI y sufijo de dirección")]
        [EndpointDescription("Decisión provisional: últimos tres caracteres de dirección RENIEC, con espacios exteriores retirados y sin distinguir mayúsculas. Requiere nombres verificables y dirección con al menos tres caracteres. Devuelve token Bearer de corta duración; no verifica correo/celular ni consentimiento del otro cónyuge.")]
        [ProducesResponseType<SesionCiudadanaDto>(200)]
        [ProducesResponseType<ValidationProblemDetails>(400)]
        [ProducesResponseType<ProblemDetails>(401)]
        [ProducesResponseType<ProblemDetails>(429)]
        [ProducesResponseType<ProblemDetails>(503)]
        public async Task<ActionResult<SesionCiudadanaDto>> IniciarSesion([FromBody] IniciarSesionCiudadanaDto datos, CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "no-store";
            emisor.ComprobarConfiguracion();
            var ciudadano = await servicio.IniciarSesionAsync(datos, cancellationToken);
            return Ok(emisor.Emitir(ciudadano));
        }

        [Authorize, HttpGet("sesion")]
        [EndpointSummary("Consultar la cuenta de la sesión actual")]
        [ProducesResponseType<CiudadanoAutenticadoDto>(200)]
        [ProducesResponseType<ProblemDetails>(401)]
        public async Task<ActionResult<CiudadanoAutenticadoDto>> ConsultarSesion(CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "no-store";
            if (!long.TryParse(User.FindFirst("sub")?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id <= 0)
                return Unauthorized();
            var ciudadano = await servicio.ObtenerSesionAsync(id, cancellationToken);
            return ciudadano is null ? Unauthorized() : Ok(ciudadano);
        }
    }
}
