using System.ComponentModel.DataAnnotations;
using Divorcios.Negocio.DTOs.Catalogos;
using Divorcios.Negocio.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Divorcios.Api.Controllers
{
    [ApiController]
    [Route("api/catalogos")]
    [Tags("Catálogos")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public sealed class CatalogosController(ICatalogosServicio servicio) : ControllerBase
    {
        [HttpGet("estados-expediente")]
        [EndpointSummary("Listar estados expediente")]
        [EndpointDescription("Sin activo devuelve todos; activo=true o false filtra por ese indicador. No evalúa vigencia ni aplicabilidad al trámite.")]
        [ProducesResponseType<IReadOnlyList<EstadoExpedienteDto>>(StatusCodes.Status200OK, "application/json")]
        public async Task<ActionResult<IReadOnlyList<EstadoExpedienteDto>>> ListarEstadosExpediente(
            [FromQuery] bool? activo, CancellationToken cancellationToken)
        {
            return Ok(await servicio.ListarEstadosExpedienteAsync(activo, cancellationToken));
        }

        [HttpGet("estados-expediente/{id:int}")]
        [EndpointSummary("Consultar estados expediente por identificador")]
        [ProducesResponseType<EstadoExpedienteDto>(StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
        public async Task<ActionResult<EstadoExpedienteDto>> ObtenerEstadoExpediente(
            [Range(1, short.MaxValue)] int id, CancellationToken cancellationToken)
        {
            var registro = await servicio.ObtenerEstadoExpedienteAsync(id, cancellationToken);
            return registro is null
                ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Registro de catálogo no encontrado.")
                : Ok(registro);
        }

        [HttpGet("tipos-documento")]
        [EndpointSummary("Listar tipos documento")]
        [EndpointDescription("Sin activo devuelve todos; activo=true o false filtra por ese indicador. No evalúa vigencia ni aplicabilidad al trámite.")]
        [ProducesResponseType<IReadOnlyList<TipoDocumentoDto>>(StatusCodes.Status200OK, "application/json")]
        public async Task<ActionResult<IReadOnlyList<TipoDocumentoDto>>> ListarTiposDocumento(
            [FromQuery] bool? activo, CancellationToken cancellationToken)
        {
            return Ok(await servicio.ListarTiposDocumentoAsync(activo, cancellationToken));
        }

        [HttpGet("tipos-documento/{id:int}")]
        [EndpointSummary("Consultar tipos documento por identificador")]
        [ProducesResponseType<TipoDocumentoDto>(StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
        public async Task<ActionResult<TipoDocumentoDto>> ObtenerTipoDocumento(
            [Range(1, short.MaxValue)] int id, CancellationToken cancellationToken)
        {
            var registro = await servicio.ObtenerTipoDocumentoAsync(id, cancellationToken);
            return registro is null
                ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Registro de catálogo no encontrado.")
                : Ok(registro);
        }

        [HttpGet("requisitos")]
        [EndpointSummary("Listar requisitos")]
        [EndpointDescription("Sin activo devuelve todos; activo=true o false filtra por ese indicador. No evalúa vigencia ni aplicabilidad al trámite.")]
        [ProducesResponseType<IReadOnlyList<RequisitoCatalogoDto>>(StatusCodes.Status200OK, "application/json")]
        public async Task<ActionResult<IReadOnlyList<RequisitoCatalogoDto>>> ListarRequisitos(
            [FromQuery] bool? activo, CancellationToken cancellationToken)
        {
            return Ok(await servicio.ListarRequisitosAsync(activo, cancellationToken));
        }

        [HttpGet("requisitos/{id:int}")]
        [EndpointSummary("Consultar requisitos por identificador")]
        [ProducesResponseType<RequisitoCatalogoDto>(StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
        public async Task<ActionResult<RequisitoCatalogoDto>> ObtenerRequisitoCatalogo(
            [Range(1, short.MaxValue)] int id, CancellationToken cancellationToken)
        {
            var registro = await servicio.ObtenerRequisitoCatalogoAsync(id, cancellationToken);
            return registro is null
                ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Registro de catálogo no encontrado.")
                : Ok(registro);
        }

        [HttpGet("destinos-oficio")]
        [EndpointSummary("Listar destinos oficio")]
        [EndpointDescription("Sin activo devuelve todos; activo=true o false filtra por ese indicador. No evalúa vigencia ni aplicabilidad al trámite.")]
        [ProducesResponseType<IReadOnlyList<DestinoOficioDto>>(StatusCodes.Status200OK, "application/json")]
        public async Task<ActionResult<IReadOnlyList<DestinoOficioDto>>> ListarDestinosOficio(
            [FromQuery] bool? activo, CancellationToken cancellationToken)
        {
            return Ok(await servicio.ListarDestinosOficioAsync(activo, cancellationToken));
        }

        [HttpGet("destinos-oficio/{id:int}")]
        [EndpointSummary("Consultar destinos oficio por identificador")]
        [ProducesResponseType<DestinoOficioDto>(StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
        public async Task<ActionResult<DestinoOficioDto>> ObtenerDestinoOficio(
            [Range(1, short.MaxValue)] int id, CancellationToken cancellationToken)
        {
            var registro = await servicio.ObtenerDestinoOficioAsync(id, cancellationToken);
            return registro is null
                ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Registro de catálogo no encontrado.")
                : Ok(registro);
        }

        [HttpGet("reglas-plazo")]
        [EndpointSummary("Listar reglas plazo")]
        [EndpointDescription("Sin activo devuelve todos; activo=true o false filtra por ese indicador. No evalúa vigencia ni aplicabilidad al trámite.")]
        [ProducesResponseType<IReadOnlyList<ReglaPlazoDto>>(StatusCodes.Status200OK, "application/json")]
        public async Task<ActionResult<IReadOnlyList<ReglaPlazoDto>>> ListarReglasPlazo(
            [FromQuery] bool? activo, CancellationToken cancellationToken)
        {
            return Ok(await servicio.ListarReglasPlazoAsync(activo, cancellationToken));
        }

        [HttpGet("reglas-plazo/{id:int}")]
        [EndpointSummary("Consultar reglas plazo por identificador")]
        [ProducesResponseType<ReglaPlazoDto>(StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
        public async Task<ActionResult<ReglaPlazoDto>> ObtenerReglaPlazo(
            [Range(1, short.MaxValue)] int id, CancellationToken cancellationToken)
        {
            var registro = await servicio.ObtenerReglaPlazoAsync(id, cancellationToken);
            return registro is null
                ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Registro de catálogo no encontrado.")
                : Ok(registro);
        }

        [HttpGet("dias-no-laborables")]
        [EndpointSummary("Listar dias no laborables")]
        [EndpointDescription("Sin activo devuelve todos; activo=true o false filtra por ese indicador. No evalúa vigencia ni aplicabilidad al trámite.")]
        [ProducesResponseType<IReadOnlyList<DiaNoLaborableDto>>(StatusCodes.Status200OK, "application/json")]
        public async Task<ActionResult<IReadOnlyList<DiaNoLaborableDto>>> ListarDiasNoLaborables(
            [FromQuery] bool? activo, CancellationToken cancellationToken)
        {
            return Ok(await servicio.ListarDiasNoLaborablesAsync(activo, cancellationToken));
        }

        [HttpGet("dias-no-laborables/{id:int}")]
        [EndpointSummary("Consultar dias no laborables por identificador")]
        [ProducesResponseType<DiaNoLaborableDto>(StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
        public async Task<ActionResult<DiaNoLaborableDto>> ObtenerDiaNoLaborable(
            [Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
        {
            var registro = await servicio.ObtenerDiaNoLaborableAsync(id, cancellationToken);
            return registro is null
                ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Registro de catálogo no encontrado.")
                : Ok(registro);
        }
    }
}
