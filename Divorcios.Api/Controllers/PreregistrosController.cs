using Divorcios.Api.Seguridad;
using Divorcios.Negocio.DTOs.Comun;
using Divorcios.Negocio.DTOs.Preregistro;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Validaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Divorcios.Api.Controllers
{
    [ApiController, Authorize, Route("api/preregistros"), Tags("Prerregistro · Cabecera")]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType<ProblemDetails>(401)]
    public sealed class PreregistrosController(IPreregistrosServicio servicio) : ControllerBase
    {
        [HttpPost]
        [EndpointSummary("Crear el prerregistro digital (C01 · paquete B)")]
        [EndpointDescription("Exactamente dos personas identificadas, posiciones A/B y un iniciador correspondiente a la sesión. Guarda expediente, participantes, PRE y auditoría juntos. No crea encuesta, cuenta del otro cónyuge ni número de Mesa de Partes. No admite campos adicionales de actor/estado/fechas. No habilitar reintentos automáticos: falta acordar idempotencia de creación.")]
        [ProducesResponseType<PreregistroDetalleDto>(201)]
        [ProducesResponseType<ProblemDetails>(409)]
        public async Task<ActionResult<PreregistroDetalleDto>> Crear([FromBody] CrearPreregistroDto datos, CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "no-store";
            var creado = await servicio.CrearAsync(ActorCiudadanoHttp.Obtener(HttpContext), datos, cancellationToken);
            return CreatedAtAction(nameof(Obtener), new { preregistroId = creado.PreregistroId }, creado);
        }

        [HttpGet]
        [EndpointSummary("Listar los prerregistros del ciudadano (C02 · paquete B)")]
        [EndpointDescription("Incluye participación como iniciador u otro cónyuge; se filtra por la persona de la sesión. pagina=1 y tamanoPagina=20 por defecto, máximo 100. Orden creación descendente e ID descendente. Una página sin resultados devuelve 200 y items vacío. No recibe cuenta/persona arbitrarias como filtro.")]
        [ProducesResponseType<PaginaDto<PreregistroResumenDto>>(200)]
        public async Task<ActionResult<PaginaDto<PreregistroResumenDto>>> Listar([FromQuery] ListarPreregistrosDto filtro, CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "no-store";
            return Ok(await servicio.ListarAsync(ActorCiudadanoHttp.Obtener(HttpContext), filtro, cancellationToken));
        }

        [HttpGet("{preregistroId}")]
        [EndpointSummary("Consultar la cabecera de un prerregistro (C03 · paquete B)")]
        [EndpointDescription("Sólo participantes autenticados. Un recurso inexistente o ajeno devuelve 404. Las referencias históricas se derivan de auditoría/revisiones; una referencia no acreditada queda null y se señala en referenciasPendientes. CREAR_VERSION está disponible para el iniciador en BORRADOR/OBSERVADO sin bloqueo ni revisión abierta.")]
        [ProducesResponseType<PreregistroDetalleDto>(200)]
        [ProducesResponseType<ProblemDetails>(404)]
        public async Task<ActionResult<PreregistroDetalleDto>> Obtener(
            [FromRoute, IdentificadorPositivo] string preregistroId, CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "no-store";
            return Ok(await servicio.ObtenerAsync(ActorCiudadanoHttp.Obtener(HttpContext), preregistroId, cancellationToken));
        }
    }
}
