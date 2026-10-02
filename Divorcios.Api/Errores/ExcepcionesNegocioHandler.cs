using System.ComponentModel.DataAnnotations;
using Divorcios.Negocio.Excepciones;
using Microsoft.AspNetCore.Diagnostics;

namespace Divorcios.Api.Errores
{
    public sealed class ExcepcionesNegocioHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext contexto, Exception excepcion, CancellationToken cancellationToken)
        {
            var error = excepcion switch
            {
                AccesoCiudadanoRechazadoException => (401, "ACCESO_RECHAZADO", excepcion.Message),
                ArchivoNegocioException archivo => (archivo.EstadoHttp, archivo.Codigo, archivo.Message),
                BadHttpRequestException { StatusCode: 413 } => (413, "ARCHIVO_DEMASIADO_GRANDE", "La carga supera el tamaño permitido."),
                AccesoDenegadoNegocioException => (403, "ACCESO_DENEGADO", excepcion.Message),
                ServicioNoDisponibleNegocioException noDisponible => (503, noDisponible.Codigo, noDisponible.Message),
                LimiteConsultasNegocioException => (429, "LIMITE_CONSULTAS", excepcion.Message),
                RecursoNoEncontradoNegocioException => (404, "RECURSO_NO_ENCONTRADO", excepcion.Message),
                ConflictoNegocioException => (409, "CONFLICTO_OPERACION", excepcion.Message),
                ValidationException => (400, "DATOS_INVALIDOS", excepcion.Message),
                _ => (0, "", "")
            };
            if (error.Item1 == 0) return false;
            await ProblemasApi.EscribirAsync(contexto, error.Item1, error.Item2, error.Item3);
            return true;
        }
    }
}
