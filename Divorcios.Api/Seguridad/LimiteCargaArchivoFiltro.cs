using Divorcios.Api.Errores;
using Divorcios.Negocio.Interfaces;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Divorcios.Api.Seguridad
{
    public sealed class LimiteCargaArchivoFiltro(IInformacionPreregistroServicio informacion) : IAsyncResourceFilter
    {
        public async Task OnResourceExecutionAsync(ResourceExecutingContext contexto, ResourceExecutionDelegate siguiente)
        {
            var politica = informacion.Obtener().PoliticaArchivos;
            if (politica is null || politica.MaximoBytes > long.MaxValue - 65536)
            {
                await Rechazar(contexto, 503, "POLITICA_ARCHIVOS_PENDIENTE", "La política de carga de archivos no está configurada.");
                return;
            }
            // Margen técnico para cabeceras y campos multipart; no aumenta el límite de bytes del archivo.
            var maximoSolicitud = politica.MaximoBytes + 65536;
            var peticion = contexto.HttpContext.Request;
            if (peticion.ContentLength > maximoSolicitud)
            {
                await Rechazar(contexto, 413, "ARCHIVO_DEMASIADO_GRANDE", "La carga supera el tamaño permitido.");
                return;
            }
            if (contexto.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>() is { IsReadOnly: false } limite)
                limite.MaxRequestBodySize = maximoSolicitud;
            contexto.HttpContext.Features.Set<IFormFeature>(new FormFeature(peticion, new FormOptions
            {
                MultipartBodyLengthLimit = maximoSolicitud, ValueLengthLimit = 4096, ValueCountLimit = 8
            }));
            try
            {
                var formulario = await peticion.ReadFormAsync(contexto.HttpContext.RequestAborted);
                if (formulario.Files.Count != 1 || !formulario.Files[0].Name.Equals("archivo", StringComparison.OrdinalIgnoreCase))
                {
                    await Rechazar(contexto, 400, "DATOS_INVALIDOS", "Adjunte exactamente un archivo en el campo archivo.");
                    return;
                }
            }
            catch (InvalidDataException e)
            {
                var grande = e.Message.Contains("length limit", StringComparison.OrdinalIgnoreCase);
                await Rechazar(contexto, grande ? 413 : 400, grande ? "ARCHIVO_DEMASIADO_GRANDE" : "DATOS_INVALIDOS",
                    grande ? "La carga supera el tamaño permitido." : "El formulario multipart no es válido.");
                return;
            }
            await siguiente();
        }
        private static async Task Rechazar(ResourceExecutingContext contexto, int estado, string codigo, string mensaje)
        {
            await ProblemasApi.EscribirAsync(contexto.HttpContext, estado, codigo, mensaje);
            contexto.Result = new EmptyResult();
        }
    }
}
