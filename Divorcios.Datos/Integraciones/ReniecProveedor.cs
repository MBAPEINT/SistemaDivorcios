using System.Security.Cryptography;
using System.Text.Json;
using Divorcios.Datos.Interfaces;
using Divorcios.Datos.Opciones;
using Divorcios.Datos.Resultados;
using Microsoft.Extensions.Options;

namespace Divorcios.Datos.Integraciones
{
    public sealed class ReniecProveedor(HttpClient cliente, IOptions<ReniecOpciones> opciones) : IReniecProveedor
    {
        // Host fijo de la integración autorizada. Nunca registrar esta URI con sus parámetros.
        private const string Endpoint = "https://apps.muniporvenir.gob.pe:8000/webservice/consulta_dni/";

        public async Task<RespuestaReniec> ConsultarAsync(string dni, CancellationToken cancellationToken)
        {
            var configuracion = opciones.Value;
            if (!configuracion.EsValida)
                return Error();

            using var tiempo = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            tiempo.CancelAfter(TimeSpan.FromSeconds(configuracion.TiempoEsperaSegundos));
            short? codigoHttp = null;
            string? hash = null;
            try
            {
                var uri = $"{Endpoint}?dni={Uri.EscapeDataString(dni)}&user={Uri.EscapeDataString(configuracion.Usuario!)}&pass={Uri.EscapeDataString(configuracion.Clave!)}";
                using var respuesta = await cliente.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, tiempo.Token);
                codigoHttp = (short)respuesta.StatusCode;
                // Acotar una respuesta inesperada sin conservar el JSON bruto ni los secretos.
                await using var flujo = await respuesta.Content.ReadAsStreamAsync(tiempo.Token);
                using var contenido = new MemoryStream();
                var bloque = new byte[4096];
                int leidos;
                while ((leidos = await flujo.ReadAsync(bloque, tiempo.Token)) > 0)
                {
                    if (contenido.Length + leidos > 16384) return Error(codigoHttp);
                    contenido.Write(bloque, 0, leidos);
                }
                var bytes = contenido.ToArray();
                hash = Convert.ToHexStringLower(SHA256.HashData(bytes));
                if (!respuesta.IsSuccessStatusCode) return Error(codigoHttp, hash);

                using var json = JsonDocument.Parse(bytes);
                var raiz = json.RootElement;
                var recibido = Campo(raiz, "dni");
                var nombres = Campo(raiz, "prenombres");
                var paterno = Campo(raiz, "apPrimer");
                var materno = Campo(raiz, "apSegundo");
                var direccion = Campo(raiz, "direccion");
                if (recibido != dni || !Valido(nombres, 120) || !Valido(paterno, 80)
                    || !Valido(materno, 80) || direccion?.Length > 250)
                    return Error(codigoHttp, hash);

                return new(true, nombres, paterno, materno, direccion, codigoHttp, hash);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return Error(codigoHttp, hash);
            }
            catch (HttpRequestException)
            {
                return Error(codigoHttp, hash);
            }
            catch (JsonException)
            {
                return Error(codigoHttp, hash);
            }
            catch (IOException)
            {
                return Error(codigoHttp, hash);
            }
        }

        private static string? Campo(JsonElement raiz, string nombre)
        {
            if (raiz.ValueKind != JsonValueKind.Object || !raiz.TryGetProperty(nombre, out var campo)
                || campo.ValueKind != JsonValueKind.String) return null;
            var valor = campo.GetString()?.Trim();
            return string.IsNullOrEmpty(valor) ? null : valor;
        }

        private static bool Valido(string? valor, int maximo) => !string.IsNullOrWhiteSpace(valor) && valor.Length <= maximo;
        private static RespuestaReniec Error(short? codigo = null, string? hash = null) => new(false, null, null, null, null, codigo, hash);
    }
}
