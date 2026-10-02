using Divorcios.Datos.Resultados;
using Divorcios.Negocio.Resultados;
using System.Globalization;
using System.Text.Json;

namespace Divorcios.Negocio.Servicios
{
    internal static class EvidenciaGeneracionRequisitos
    {
        public static GeneracionRequisitosResultado Leer(LecturaVersionesPreregistro lectura, long versionId)
        {
            var id = versionId.ToString(CultureInfo.InvariantCulture);
            var evento = lectura.Eventos.Where(x => x.RecursoId == id && x.AccionCodigo == "PREREGISTRO_VERSION_CREADA")
                .OrderBy(x => x.RegistroAuditoriaId).FirstOrDefault();
            var desconocida = new GeneracionRequisitosResultado("SIN_ACREDITAR", null,
                ["MATRIZ_RESPUESTA_REQUISITO", "TIPOS_Y_CANTIDAD_DOCUMENTAL"], []);
            if (evento?.DetalleJson is null) return desconocida;
            try
            {
                using var json = JsonDocument.Parse(evento.DetalleJson);
                var raiz = json.RootElement;
                if (raiz.ValueKind != JsonValueKind.Object || !raiz.TryGetProperty("estadoGeneracionRequisitosCodigo", out var estado)
                    || estado.ValueKind != JsonValueKind.String || estado.GetString() is not ("PENDIENTE_CONFIGURACION" or "GENERACION_PARCIAL"))
                    return desconocida;
                var pendientes = raiz.TryGetProperty("pendientesGeneracionRequisitos", out var p)
                    ? p.Deserialize<string[]>() : ["MATRIZ_RESPUESTA_REQUISITO", "TIPOS_Y_CANTIDAD_DOCUMENTAL"];
                var matriz = raiz.TryGetProperty("matrizVersion", out var m) && m.ValueKind == JsonValueKind.String ? m.GetString() : null;
                var requisitos = raiz.TryGetProperty("requisitosGenerados", out var r) ? r.Deserialize<RequisitoGenerado[]>() : [];
                if (pendientes is null || requisitos is null || pendientes.Any(string.IsNullOrWhiteSpace)
                    || requisitos.Any(x => x is null || x.Definicion is null || x.Definicion.TiposDocumentoCodigos is null || x.Definicion.Fuentes is null)
                    || requisitos.Select(x => x.PreregistroRequisitoId).Distinct().Count() != requisitos.Length
                    || (estado.GetString() == "GENERACION_PARCIAL" && (string.IsNullOrWhiteSpace(matriz) || pendientes.Length == 0 || requisitos.Length == 0)))
                    return desconocida;
                return new(estado.GetString()!, matriz, pendientes, requisitos);
            }
            catch (JsonException) { return desconocida; }
        }
    }
}
