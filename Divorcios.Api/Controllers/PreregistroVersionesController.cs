using Divorcios.Api.Seguridad;
using Divorcios.Negocio.DTOs.Preregistro;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Validaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Divorcios.Api.Controllers
{
    [ApiController, Authorize, Route("api/preregistros/{preregistroId}/versiones"), Tags("Prerregistro · Encuestas")]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType<ProblemDetails>(401)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(409)]
    public sealed class PreregistroVersionesController(IVersionesPreregistroServicio servicio) : ControllerBase
    {
        [HttpPost]
        [EndpointSummary("Guardar una nueva encuesta y sus contactos (V01 · paquete C)")]
        [EndpointDescription("Sólo iniciador. Snapshot completo e inmutable; campos obligatorios explícitos y tres respuestas nullable pendientes. Primera versión: versionBaseId=null; siguientes: ID de la última y motivo obligatorio. Versiones superadas producen 409. BORRADOR/OBSERVADO habilitados, sin revisión abierta ni formalización. No reabre APROBADO. Guarda encuesta, contactos, requisitos sustentados disponibles y auditoría en una transacción. Generación parcial con motivos pendientes, o PENDIENTE_CONFIGURACION si faltan catálogos/configuración. Nunca acredita completitud. No copia archivos de encuestas anteriores.")]
        [ProducesResponseType<PreregistroVersionDto>(201)]
        [ProducesResponseType<ProblemDetails>(403)]
        public async Task<ActionResult<PreregistroVersionDto>> Crear([FromRoute, IdentificadorPositivo] string preregistroId,
            [FromBody] CrearVersionPreregistroDto datos, CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "no-store";
            var version = await servicio.CrearAsync(ActorCiudadanoHttp.Obtener(HttpContext), preregistroId, datos, cancellationToken);
            return CreatedAtAction(nameof(Obtener), new { preregistroId, versionId = version.PreregistroVersionId }, version);
        }

        [HttpGet]
        [EndpointSummary("Listar las encuestas históricas (V02 · paquete C)")]
        [EndpointDescription("Ambos cónyuges autenticados. Número de versión descendente, referencias de revisión y envío acreditado por eventos/revisiones. enviada=null si la historia no permite determinarlo. No se deduce el envío de la última encuesta. Sin encuestas devuelve [].")]
        [ProducesResponseType<IReadOnlyList<PreregistroVersionResumenDto>>(200)]
        public async Task<ActionResult<IReadOnlyList<PreregistroVersionResumenDto>>> Listar(
            [FromRoute, IdentificadorPositivo] string preregistroId, CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "no-store";
            return Ok(await servicio.ListarAsync(ActorCiudadanoHttp.Obtener(HttpContext), preregistroId, cancellationToken));
        }

        [HttpGet("{versionId}")]
        [EndpointSummary("Consultar una encuesta exacta y sus contactos históricos (V03 · paquete C)")]
        [EndpointDescription("Respuestas conservadas, contactos vigentes al crear esa encuesta y estado de generación de requisitos. editable indica si el iniciador puede trabajar sobre esa versión; no existe PUT/PATCH para sobrescribir respuestas. Una versión ajena al PRE produce 404.")]
        [ProducesResponseType<PreregistroVersionDto>(200)]
        public async Task<ActionResult<PreregistroVersionDto>> Obtener([FromRoute, IdentificadorPositivo] string preregistroId,
            [FromRoute, IdentificadorPositivo] string versionId, CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "no-store";
            return Ok(await servicio.ObtenerAsync(ActorCiudadanoHttp.Obtener(HttpContext), preregistroId, versionId, cancellationToken));
        }

        [HttpGet("{versionId}/requisitos")]
        [EndpointSummary("Consultar requisitos y evidencia presentada de una encuesta (V04 · paquete C)")]
        [EndpointDescription("Respuesta envolvente con items, estadoGeneracionCodigo y pendientesConfiguracion: lista vacía o generación parcial NO acreditan completitud ni aprobación. Consulta requisitos existentes, incluso con catálogo desactivado; no genera nuevos. Los generados conservan determinacion con versión de matriz, nombre al generar, titulares, cobertura, tipos alternativos y fuentes. Metadatos de cada DocumentoVersion seleccionada, sin rutas ni sustitución por una corrección más reciente. Presentación no equivale a evaluación de la abogada.")]
        [ProducesResponseType<RequisitosVersionPreregistroDto>(200)]
        public async Task<ActionResult<RequisitosVersionPreregistroDto>> Requisitos([FromRoute, IdentificadorPositivo] string preregistroId,
            [FromRoute, IdentificadorPositivo] string versionId, CancellationToken cancellationToken)
        {
            Response.Headers.CacheControl = "no-store";
            return Ok(await servicio.ObtenerRequisitosAsync(ActorCiudadanoHttp.Obtener(HttpContext), preregistroId, versionId, cancellationToken));
        }
    }
}
