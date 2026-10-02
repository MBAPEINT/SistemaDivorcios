using Divorcios.Negocio.DTOs.Identidad;
using Divorcios.Negocio.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Divorcios.Api.Controllers
{
    [ApiController, Route("api/identidad"), Tags("Identidad")]
    public sealed class IdentidadController(IIdentidadServicio servicio) : ControllerBase
    {
        [Authorize(Policy = "ConsultarIdentidad"), EnableRateLimiting("ConsultasIdentidad"), HttpPost("consultas-dni")]
        [EndpointSummary("Consultar identidad con caché RENIEC (P02)")]
        [EndpointDescription("Requiere Authorization: Bearer. No autentica al solicitante ni crea cuentas del DNI consultado. No devuelve la dirección. La respuesta incompleta del proveedor produce 503, no una persona verificada.")]
        [ProducesResponseType<ConsultaIdentidadDto>(200)]
        [ProducesResponseType<ValidationProblemDetails>(400)]
        [ProducesResponseType<ProblemDetails>(401)]
        [ProducesResponseType<ProblemDetails>(403)]
        [ProducesResponseType<ProblemDetails>(429)]
        [ProducesResponseType<ProblemDetails>(503)]
        public async Task<ActionResult<ConsultaIdentidadDto>> Consultar([FromBody] ConsultarDniDto datos, CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "no-store";
            return Ok((await servicio.ConsultarAsync(datos, cancellationToken)).Datos);
        }
    }
}
