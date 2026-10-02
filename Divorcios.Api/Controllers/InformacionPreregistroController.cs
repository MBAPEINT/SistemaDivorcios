using Divorcios.Negocio.DTOs.Preregistro;
using Divorcios.Negocio.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Divorcios.Api.Controllers
{
    [ApiController, Route("api/preregistro"), Tags("Prerregistro · Información")]
    public sealed class InformacionPreregistroController(IInformacionPreregistroServicio servicio) : ControllerBase
    {
        [AllowAnonymous, HttpGet("informacion")]
        [EndpointSummary("Consultar orientación pública del prerregistro (P01)")]
        [EndpointDescription("No crea registros ni consulta RENIEC. Formatos vacíos y política null indican configuración pendiente; no significan archivos ilimitados.")]
        [ProducesResponseType<InformacionPreregistroDto>(StatusCodes.Status200OK)]
        public ActionResult<InformacionPreregistroDto> Obtener() => Ok(servicio.Obtener());
    }
}
