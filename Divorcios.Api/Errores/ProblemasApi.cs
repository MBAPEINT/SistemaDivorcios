using Microsoft.AspNetCore.Mvc;

namespace Divorcios.Api.Errores
{
    public static class ProblemasApi
    {
        public static ProblemDetails Crear(HttpContext contexto, int estado, string codigo, string titulo)
        {
            var problema = new ProblemDetails { Status = estado, Title = titulo, Instance = contexto.Request.Path };
            problema.Extensions["codigo"] = codigo;
            problema.Extensions["traceId"] = contexto.TraceIdentifier;
            return problema;
        }

        public static async Task EscribirAsync(HttpContext contexto, int estado, string codigo, string titulo)
        {
            contexto.Response.StatusCode = estado;
            await contexto.Response.WriteAsJsonAsync(Crear(contexto, estado, codigo, titulo),
                options: (System.Text.Json.JsonSerializerOptions?)null,
                contentType: "application/problem+json", cancellationToken: contexto.RequestAborted);
        }
    }
}
