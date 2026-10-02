using Divorcios.Api.Modelos;
using Divorcios.Api.Seguridad;
using Divorcios.Negocio.DTOs.Comun;
using Divorcios.Negocio.DTOs.Preregistro;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Resultados;
using Divorcios.Negocio.Validaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Divorcios.Api.Controllers
{
    [ApiController, Authorize, Route("api/preregistros/{preregistroId}"), Tags("Prerregistro · Archivos")]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType<ProblemDetails>(401)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(409)]
    [ProducesResponseType<ProblemDetails>(503)]
    public sealed class PreregistroArchivosController(IArchivosPreregistroServicio servicio) : ControllerBase
    {
        [HttpPost("versiones/{versionId}/requisitos/{requisitoId}/documentos")]
        [Consumes("multipart/form-data"), ServiceFilter(typeof(LimiteCargaArchivoFiltro))]
        [EndpointSummary("Cargar un nuevo documento y presentarlo (D01 · paquete D)")]
        [EndpointDescription("Un PDF por petición; máximo configurable publicado en P01. Sólo iniciador, requisito de la última encuesta sin enviar/evaluar. tipoDocumentoId y titulo; estructura PDF analizada, tamaño/hash calculados por el servidor. Crea documento con origen fijo, archivo inmutable y vínculo presentado; no acredita conformidad. Requiere correspondencia requisito-tipo confirmada y respeta la determinación histórica. No crea catálogos. 413 tamaño, 415 extensión/contenido no admitidos, 503 configuración/almacenamiento pendientes.")]
        [ProducesResponseType<DocumentoVersionDto>(201)]
        [ProducesResponseType<ProblemDetails>(403)]
        [ProducesResponseType<ProblemDetails>(413)]
        [ProducesResponseType<ProblemDetails>(415)]
        public async Task<ActionResult<DocumentoVersionDto>> Cargar([FromRoute, IdentificadorPositivo] string preregistroId,
            [FromRoute, IdentificadorPositivo] string versionId, [FromRoute, IdentificadorPositivo] string requisitoId,
            [FromForm] CargarArchivoPreregistroFormulario datos, CancellationToken cancellationToken)
        {
            NoCache();
            ValidarCampos(["tipoDocumentoId", "titulo"]);
            await using var stream = datos.Archivo.OpenReadStream();
            var archivo = await servicio.CargarAsync(ActorCiudadanoHttp.Obtener(HttpContext), preregistroId, versionId,
                requisitoId, datos.TipoDocumentoId!.Value, datos.Titulo,
                new ArchivoEntrada(stream, datos.Archivo.FileName, datos.Archivo.Length), cancellationToken);
            return Created(archivo.UrlDescarga, archivo);
        }

        [HttpPost("versiones/{versionId}/requisitos/{requisitoId}/documentos/{documentoId}/versiones")]
        [Consumes("multipart/form-data"), ServiceFilter(typeof(LimiteCargaArchivoFiltro))]
        [EndpointSummary("Cargar una corrección del mismo documento (D02 · paquete D)")]
        [EndpointDescription("archivo, motivoCambio y documentoVersionBaseId obligatorio (última versión lógica del documento, obtenida en D04). Si cambia esa base devuelve 409. El documento debe estar presentado en este requisito de trabajo; para una nueva encuesta reutilice primero con D03. Nueva clave, hash y número; sólo sustituye su selección en ese requisito. No mueve origen, presentaciones de otras encuestas/requisitos ni evaluaciones de la abogada.")]
        [ProducesResponseType<DocumentoVersionDto>(201)]
        [ProducesResponseType<ProblemDetails>(403)]
        [ProducesResponseType<ProblemDetails>(413)]
        [ProducesResponseType<ProblemDetails>(415)]
        public async Task<ActionResult<DocumentoVersionDto>> Corregir([FromRoute, IdentificadorPositivo] string preregistroId,
            [FromRoute, IdentificadorPositivo] string versionId, [FromRoute, IdentificadorPositivo] string requisitoId,
            [FromRoute, IdentificadorPositivo] string documentoId, [FromForm] CorregirArchivoPreregistroFormulario datos, CancellationToken cancellationToken)
        {
            NoCache();
            ValidarCampos(["motivoCambio", "documentoVersionBaseId"]);
            await using var stream = datos.Archivo.OpenReadStream();
            var archivo = await servicio.CorregirAsync(ActorCiudadanoHttp.Obtener(HttpContext), preregistroId, versionId,
                requisitoId, documentoId, datos.DocumentoVersionBaseId, datos.MotivoCambio,
                new ArchivoEntrada(stream, datos.Archivo.FileName, datos.Archivo.Length), cancellationToken);
            return Created(archivo.UrlDescarga, archivo);
        }

        [HttpPut("versiones/{versionId}/requisitos/{requisitoId}/archivos-presentados")]
        [EndpointSummary("Seleccionar o reutilizar archivos exactos (D03 · paquete D)")]
        [EndpointDescription("Reemplaza sólo la selección del requisito de trabajo. [] retira vínculos, conservando archivos y evaluaciones. Idempotente, máximo técnico 100 IDs por operación; no fija cantidad documental oficial. Varios documentos, máximo una versión por documento. Nuevas asociaciones requieren pertenencia, tipo pertinente configurado, vigencia e integridad. Encuesta enviada/evaluada/superada queda congelada. Escrituras concurrentes se serializan; prevalece el último conjunto guardado.")]
        [ProducesResponseType<PreregistroRequisitoDto>(200)]
        [ProducesResponseType<ProblemDetails>(403)]
        public async Task<ActionResult<PreregistroRequisitoDto>> Seleccionar([FromRoute, IdentificadorPositivo] string preregistroId,
            [FromRoute, IdentificadorPositivo] string versionId, [FromRoute, IdentificadorPositivo] string requisitoId,
            [FromBody] SeleccionarArchivosPreregistroDto datos, CancellationToken cancellationToken)
        {
            NoCache();
            return Ok(await servicio.SeleccionarAsync(ActorCiudadanoHttp.Obtener(HttpContext), preregistroId, versionId, requisitoId, datos, cancellationToken));
        }

        [HttpGet("documentos")]
        [EndpointSummary("Consultar documentos y versiones del prerregistro (D04 · paquete D)")]
        [EndpointDescription("Ambos participantes. pagina=1/tamanoPagina=20, máximo 100; tipoDocumentoId opcional. Conserva versiones antiguas, catálogos inactivos y referencias de presentación/evaluación. No implica que un archivo sea válido para cualquier requisito. No expone rutas ni claves privadas. Listado sólo de etapa PRERREGISTRO.")]
        [ProducesResponseType<PaginaDto<DocumentoPreregistroDto>>(200)]
        public async Task<ActionResult<PaginaDto<DocumentoPreregistroDto>>> Listar([FromRoute, IdentificadorPositivo] string preregistroId,
            [FromQuery] ListarDocumentosPreregistroDto filtro, CancellationToken cancellationToken)
        {
            NoCache();
            return Ok(await servicio.ListarAsync(ActorCiudadanoHttp.Obtener(HttpContext), preregistroId, filtro, cancellationToken));
        }

        [HttpGet("documentos/{documentoId}/versiones/{documentoVersionId}/archivo")]
        [EndpointSummary("Descargar una versión exacta autorizada (D05 · paquete D)")]
        [EndpointDescription("Ambos participantes; valida PRE-documento-versión y sesión en cada descarga. Archivo observado/histórico sigue consultable. Comprueba tamaño, hash y formato almacenados antes de entregar; ausente o alterado devuelve 503. Attachment con nombre seguro, no-store y nosniff; no hay ruta pública de archivos.")]
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        public async Task<IActionResult> Descargar([FromRoute, IdentificadorPositivo] string preregistroId,
            [FromRoute, IdentificadorPositivo] string documentoId, [FromRoute, IdentificadorPositivo] string documentoVersionId,
            CancellationToken cancellationToken)
        {
            NoCache();
            Response.Headers.XContentTypeOptions = "nosniff";
            var descarga = await servicio.DescargarAsync(ActorCiudadanoHttp.Obtener(HttpContext), preregistroId, documentoId, documentoVersionId, cancellationToken);
            return File(descarga.Contenido, descarga.MimeType, descarga.NombreArchivo, enableRangeProcessing: false);
        }

        private void NoCache() => Response.Headers.CacheControl = "no-store";
        private void ValidarCampos(string[] permitidos)
        {
            if (Request.Form.Keys.Any(x => !permitidos.Contains(x, StringComparer.OrdinalIgnoreCase)))
                throw new System.ComponentModel.DataAnnotations.ValidationException("El formulario contiene campos adicionales no admitidos.");
        }
    }
}
