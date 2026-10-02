using System.Net;
using System.Text;
using Divorcios.Datos.Integraciones;
using Divorcios.Datos.Opciones;
using Microsoft.Extensions.Options;

namespace Divorcios.Pruebas
{

    public sealed class ReniecProveedorPruebas
    {
        [Theory]
        [InlineData("{}")]
        [InlineData("[]")]
        [InlineData("no es json")]
        [InlineData("{\"dni\":\"12345678\",\"prenombres\":null,\"apPrimer\":null,\"apSegundo\":null,\"direccion\":null}")]
        [InlineData("{\"dni\":\"87654321\",\"prenombres\":\"ANA\",\"apPrimer\":\"UNO\",\"apSegundo\":\"DOS\"}")]
        [InlineData("{\"dni\":\"12345678\",\"prenombres\":\"ANA\",\"apPrimer\":\"UNO\",\"apSegundo\":null}")]
        public async Task RespuestaIncompletaOInconsistenteNoVerificaIdentidad(string contenido)
        {
            var respuesta = await Crear(contenido).ConsultarAsync("12345678", default);
            Assert.False(respuesta.Encontrado);
            Assert.Null(respuesta.Prenombres);
        }

        [Fact]
        public async Task RespuestaValidaNormalizaCamposSinExigirDireccion()
        {
            var respuesta = await Crear("{\"dni\":\"12345678\",\"prenombres\":\" ANA \",\"apPrimer\":\" UNO \",\"apSegundo\":\" DOS \",\"direccion\":null}")
                .ConsultarAsync("12345678", default);
            Assert.True(respuesta.Encontrado);
            Assert.Equal("ANA", respuesta.Prenombres);
            Assert.Null(respuesta.Direccion);
            Assert.Equal(64, respuesta.RespuestaHash!.Length);
        }

        [Theory]
        [InlineData(302)]
        [InlineData(404)]
        [InlineData(500)]
        public async Task NoConfundeUnErrorHttpConPersonaNoEncontrada(int codigo)
        {
            var respuesta = await Crear("{}", (HttpStatusCode)codigo).ConsultarAsync("12345678", default);
            Assert.False(respuesta.Encontrado);
            Assert.Equal((short)codigo, respuesta.CodigoHttp!.Value);
        }

        [Fact]
        public async Task ConfiguracionIncompletaNuncaLlamaAlProveedor()
        {
            var handler = new RespuestaFija("{}", HttpStatusCode.OK);
            var proveedor = new ReniecProveedor(new HttpClient(handler), Options.Create(new ReniecOpciones()));
            Assert.False((await proveedor.ConsultarAsync("12345678", default)).Encontrado);
            Assert.Equal(0, handler.Llamadas);
        }

        [Fact]
        public async Task RespuestaExcesivaNoSeConserva()
        {
            var resultado = await Crear(new string('x', 20000)).ConsultarAsync("12345678", default);
            Assert.False(resultado.Encontrado);
            Assert.Null(resultado.RespuestaHash);
        }

        private static ReniecProveedor Crear(string json, HttpStatusCode estado = HttpStatusCode.OK)
            => new(new HttpClient(new RespuestaFija(json, estado)), Options.Create(new ReniecOpciones
            { Habilitado = true, Usuario = "usuario-ficticio", Clave = "clave-ficticia", VigenciaCacheMinutos = 1, LimiteDiario = 3 }));

        private sealed class RespuestaFija(string json, HttpStatusCode estado) : HttpMessageHandler
        {
            public int Llamadas { get; private set; }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Llamadas++;
                return Task.FromResult(new HttpResponseMessage(estado) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
            }
        }
    }
}
