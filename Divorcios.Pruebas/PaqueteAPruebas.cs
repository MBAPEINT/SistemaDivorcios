using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Divorcios.Api.Seguridad;
using Divorcios.Datos.Contexto;
using Divorcios.Datos.Interfaces;
using Divorcios.Datos.Opciones;
using Divorcios.Datos.Resultados;
using Divorcios.Dominio.Entidades;
using Divorcios.Negocio.DTOs.Identidad;
using Divorcios.Negocio.Excepciones;
using Divorcios.Negocio.Extensiones;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Opciones;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace Divorcios.Pruebas
{

    [Collection("PostgreSQL aislado")]
    public sealed class PaqueteAPruebas(PostgresAislado postgres)
    {
        [Fact]
        public async Task EsquemaTiene18Migraciones37TablasYSinCambiosDeModelo()
        {
            await using var db = postgres.CrearContexto();
            Assert.Equal(18, (await db.Database.GetAppliedMigrationsAsync()).Count());
            Assert.False(db.Database.HasPendingModelChanges());
            await db.Database.OpenConnectionAsync();
            using var consulta = db.Database.GetDbConnection().CreateCommand();
            consulta.CommandText = "SELECT count(*) FROM information_schema.tables WHERE table_schema = 'divorcios' AND table_type = 'BASE TABLE'";
            Assert.Equal(37L, await consulta.ExecuteScalarAsync());
        }

        [Fact]
        public async Task SesionCreaUnaCuentaReutilizaPersonaYNoVerificaContactos()
        {
            var personaId = await Cache("12345001", "CALLE PRUEBA 123");
            using var servicios = Servicios();
            using var scope = servicios.CreateScope();
            var acceso = scope.ServiceProvider.GetRequiredService<IAccesoCiudadanoServicio>();
            var primero = await acceso.IniciarSesionAsync(new() { Dni = "12345001", SufijoDireccion = "123" }, default);
            var segundo = await acceso.IniciarSesionAsync(new() { Dni = "12345001", SufijoDireccion = "123" }, default);
            Assert.Equal(primero.CuentaCiudadanaId, segundo.CuentaCiudadanaId);
            Assert.Equal(personaId.ToString(), primero.PersonaId);
            await using var db = postgres.CrearContexto();
            var cuenta = await db.CuentasCiudadanas.SingleAsync(x => x.PersonaId == personaId);
            Assert.Null(cuenta.CorreoVerificadoEn);
            Assert.Null(cuenta.CelularVerificadoEn);
            Assert.Empty(await db.ValidacionesIdentidad.Where(x => x.CuentaCiudadanaId == cuenta.CuentaCiudadanaId).ToListAsync());
        }

        [Fact]
        public async Task DireccionNulaYDatosIncorrectosNoCreanCuenta()
        {
            var personaId = await Cache("12345002", null);
            using var servicios = Servicios();
            using var scope = servicios.CreateScope();
            var acceso = scope.ServiceProvider.GetRequiredService<IAccesoCiudadanoServicio>();
            await Assert.ThrowsAsync<AccesoCiudadanoRechazadoException>(() => acceso.IniciarSesionAsync(new() { Dni = "12345002", SufijoDireccion = "123" }, default));
            await using var db = postgres.CrearContexto();
            Assert.False(await db.CuentasCiudadanas.AnyAsync(x => x.PersonaId == personaId));
        }

        [Fact]
        public async Task CincoFallosSePersistenYBloqueanInclusoConSufijoCorrecto()
        {
            var personaId = await Cache("12345003", "CALLE ABC");
            using var servicios = Servicios();
            using var scope = servicios.CreateScope();
            var acceso = scope.ServiceProvider.GetRequiredService<IAccesoCiudadanoServicio>();
            var ciudadano = await acceso.IniciarSesionAsync(new() { Dni = "12345003", SufijoDireccion = "abc" }, default);
            for (var intento = 0; intento < 5; intento++)
                await Assert.ThrowsAsync<AccesoCiudadanoRechazadoException>(() => acceso.IniciarSesionAsync(new() { Dni = "12345003", SufijoDireccion = "000" }, default));
            await Assert.ThrowsAsync<AccesoCiudadanoRechazadoException>(() => acceso.IniciarSesionAsync(new() { Dni = "12345003", SufijoDireccion = "ABC" }, default));
            Assert.Null(await acceso.ObtenerSesionAsync(long.Parse(ciudadano.CuentaCiudadanaId), default));
            await using var db = postgres.CrearContexto();
            var cuenta = await db.CuentasCiudadanas.SingleAsync(x => x.PersonaId == personaId);
            Assert.Equal(5, cuenta.IntentosFallidos);
            Assert.True(cuenta.BloqueadoHasta > DateTime.UtcNow);
        }

        [Fact]
        public async Task ErrorDelProveedorPersisteSinPersonaYCuentaParaCuota()
        {
            await using var db = postgres.CrearContexto();
            var fin = DateTime.UtcNow;
            var inicio = fin.AddHours(-24);
            var limite = await db.ConsultasReniec.CountAsync(x => x.OrigenCodigo == "API"
                && x.ConsultadoEn >= inicio && x.ConsultadoEn <= fin) + 1;
            using var servicios = Servicios(habilitarProveedor: true, limite: limite);
            using var scope = servicios.CreateScope();
            var identidad = scope.ServiceProvider.GetRequiredService<IIdentidadServicio>();
            var error = await Assert.ThrowsAsync<ServicioNoDisponibleNegocioException>(() => identidad.ConsultarAsync(new() { Dni = "12345004" }, default));
            Assert.Equal("RENIEC_RESPUESTA_NO_VERIFICABLE", error.Codigo);
            await Assert.ThrowsAsync<LimiteConsultasNegocioException>(() => identidad.ConsultarAsync(new() { Dni = "12345004" }, default));
            var consulta = await db.ConsultasReniec.SingleAsync(x => x.DniConsultado == "12345004");
            Assert.Equal("ERROR", consulta.ResultadoCodigo);
            Assert.Null(consulta.PersonaId);
            Assert.Null(consulta.ExpiraEn);
            Assert.False(await db.Personas.AnyAsync(x => x.Dni == "12345004"));
        }

        [Fact]
        public async Task DosConsultasSimultaneasDelMismoDniLlamanUnaVezAlProveedor()
        {
            var proveedor = new ProveedorFicticio(encontrado: true);
            using var servicios = Servicios(habilitarProveedor: true, proveedor: proveedor);
            async Task<string> Consultar()
            {
                using var scope = servicios.CreateScope();
                return (await scope.ServiceProvider.GetRequiredService<IIdentidadServicio>()
                    .ConsultarAsync(new() { Dni = "12345005" }, default)).Datos.ConsultaReniecId;
            }
            var resultados = await Task.WhenAll(Consultar(), Consultar());
            Assert.Equal(resultados[0], resultados[1]);
            Assert.Equal(1, proveedor.Llamadas);
            await using var db = postgres.CrearContexto();
            Assert.Equal(1, await db.Personas.CountAsync(x => x.Dni == "12345005"));
        }

        [Fact]
        public async Task CacheExpiradaNoSeReutilizaNiLlamaAlProveedorDeshabilitado()
        {
            await Cache("12345007", "CALLE TEST 123", expirada: true);
            using var servicios = Servicios();
            using var scope = servicios.CreateScope();
            var error = await Assert.ThrowsAsync<ServicioNoDisponibleNegocioException>(() => scope.ServiceProvider
                .GetRequiredService<IIdentidadServicio>().ConsultarAsync(new() { Dni = "12345007" }, default));
            Assert.Equal("RENIEC_NO_CONFIGURADO", error.Codigo);
        }

        [Fact]
        public async Task DesconexionDelClienteNoEliminaElIntentoReservado()
        {
            using var cancelacion = new CancellationTokenSource();
            using var servicios = Servicios(habilitarProveedor: true, proveedor: new ProveedorCancelado(cancelacion));
            using var scope = servicios.CreateScope();
            await Assert.ThrowsAsync<OperationCanceledException>(() => scope.ServiceProvider.GetRequiredService<IIdentidadServicio>()
                .ConsultarAsync(new() { Dni = "12345008" }, cancelacion.Token));
            await using var db = postgres.CrearContexto();
            var intento = await db.ConsultasReniec.SingleAsync(x => x.DniConsultado == "12345008");
            Assert.Equal("ERROR", intento.ResultadoCodigo);
            Assert.Null(intento.PersonaId);
        }

        [Fact]
        public async Task ApiPublicaLoginBearerValidacionSwaggerYLimiteSinReniecReal()
        {
            await Cache("12345006", "AVENIDA TEST 321");
            using var fabrica = new ApiAislada(postgres);
            using var cliente = fabrica.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
            var info = await cliente.GetFromJsonAsync<JsonElement>("/api/preregistro/informacion");
            Assert.Equal(JsonValueKind.Null, info.GetProperty("politicaArchivos").ValueKind);
            Assert.Empty(info.GetProperty("formatos").EnumerateArray());
            Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.PostAsJsonAsync("/api/identidad/consultas-dni", new { dni = "12345006" })).StatusCode);
            var login = await cliente.PostAsJsonAsync("/api/acceso-ciudadano/sesion", new { dni = "12345006", sufijoDireccion = "321" });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            Assert.Equal("no-store", login.Headers.CacheControl!.ToString());
            var sesion = await login.Content.ReadFromJsonAsync<JsonElement>();
            var token = sesion.GetProperty("accessToken").GetString()!;
            cliente.DefaultRequestHeaders.Authorization = new("Bearer", token);
            Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/api/acceso-ciudadano/sesion")).StatusCode);
            var consulta = await cliente.PostAsJsonAsync("/api/identidad/consultas-dni", new { dni = "12345006" });
            var identidad = await consulta.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(identidad.GetProperty("desdeCache").GetBoolean());
            Assert.Equal(JsonValueKind.String, identidad.GetProperty("personaId").ValueKind);
            Assert.False(identidad.TryGetProperty("direccion", out _));
            Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PostAsJsonAsync("/api/identidad/consultas-dni", new { dni = "abc" })).StatusCode);
            var cuentaId = sesion.GetProperty("ciudadano").GetProperty("cuentaCiudadanaId").GetString()!;
            var personaId = sesion.GetProperty("ciudadano").GetProperty("personaId").GetString()!;
            cliente.DefaultRequestHeaders.Authorization = new("Bearer", fabrica.Token(cuentaId, personaId, expirado: true));
            Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/api/acceso-ciudadano/sesion")).StatusCode);
            cliente.DefaultRequestHeaders.Authorization = new("Bearer", fabrica.Token(cuentaId, personaId, audiencia: "otra-aplicacion"));
            Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/api/acceso-ciudadano/sesion")).StatusCode);
            cliente.DefaultRequestHeaders.Authorization = new("Bearer", fabrica.Token(cuentaId, "999999"));
            Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/api/acceso-ciudadano/sesion")).StatusCode);
            cliente.DefaultRequestHeaders.Authorization = new("Bearer", fabrica.Token(cuentaId, personaId, permiso: false));
            Assert.Equal(HttpStatusCode.Forbidden, (await cliente.PostAsJsonAsync("/api/identidad/consultas-dni", new { dni = "12345006" })).StatusCode);
            cliente.DefaultRequestHeaders.Authorization = new("Bearer", token + "x");
            Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/api/acceso-ciudadano/sesion")).StatusCode);
            cliente.DefaultRequestHeaders.Authorization = null;
            Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.PostAsJsonAsync("/api/acceso-ciudadano/sesion", new { dni = "12345006", sufijoDireccion = "000" })).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PostAsJsonAsync("/api/acceso-ciudadano/sesion", new { dni = "x", sufijoDireccion = "321" })).StatusCode);
            for (var intento = 0; intento < 2; intento++)
                await cliente.PostAsJsonAsync("/api/acceso-ciudadano/sesion", new { dni = "x", sufijoDireccion = "321" });
            Assert.Equal(HttpStatusCode.TooManyRequests, (await cliente.PostAsJsonAsync("/api/acceso-ciudadano/sesion", new { dni = "x", sufijoDireccion = "321" })).StatusCode);
            var swagger = await cliente.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
            Assert.Equal("bearer", swagger.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer").GetProperty("scheme").GetString());
            Assert.True(swagger.GetProperty("paths").GetProperty("/api/identidad/consultas-dni").GetProperty("post").TryGetProperty("security", out _));
            Assert.False(swagger.GetProperty("paths").GetProperty("/api/preregistro/informacion").GetProperty("get").TryGetProperty("security", out _));
            Assert.Equal(0, fabrica.Proveedor.Llamadas);
        }

        private ServiceProvider Servicios(bool habilitarProveedor = false, int limite = 10, IReniecProveedor? proveedor = null)
        {
            var servicios = new ServiceCollection();
            servicios.AddLogging();
            servicios.AgregarCapaNegocio(postgres.Conexion);
            servicios.Configure<AccesoCiudadanoOpciones>(x => x.HabilitarDniDireccion = true);
            servicios.Configure<ReniecOpciones>(x =>
            {
                x.Habilitado = habilitarProveedor; x.Usuario = "ficticio"; x.Clave = "ficticia";
                x.VigenciaCacheMinutos = 60; x.LimiteDiario = limite;
            });
            servicios.AddSingleton<IReniecProveedor>(proveedor ?? new ProveedorFicticio());
            return servicios.BuildServiceProvider();
        }

        private async Task<long> Cache(string dni, string? direccion, bool expirada = false)
        {
            await using var db = postgres.CrearContexto();
            var ahora = DateTime.UtcNow;
            var persona = new Persona { Dni = dni, Nombres = "DATOS FICTICIOS", ApellidoPaterno = "PRUEBA", ApellidoMaterno = "AISLADA",
                DireccionDni = direccion, VerificadoReniec = true, VerificadoReniecEn = ahora, CreadoEn = ahora };
            db.ConsultasReniec.Add(new() { Persona = persona, DniConsultado = dni, Prenombres = persona.Nombres,
                ApellidoPaterno = persona.ApellidoPaterno, ApellidoMaterno = persona.ApellidoMaterno, Direccion = direccion,
                ResultadoCodigo = "ENCONTRADO", OrigenCodigo = "IMPORTACION", ConsultadoEn = expirada ? ahora.AddHours(-2) : ahora,
                ExpiraEn = expirada ? ahora.AddHours(-1) : ahora.AddHours(1) });
            await db.SaveChangesAsync();
            return persona.PersonaId;
        }

        private sealed class ProveedorFicticio(bool encontrado = false) : IReniecProveedor
        {
            private int llamadas;
            public int Llamadas => llamadas;
            public async Task<RespuestaReniec> ConsultarAsync(string dni, CancellationToken cancellationToken)
            {
                Interlocked.Increment(ref llamadas);
                await Task.Delay(50, cancellationToken);
                return encontrado ? new(true, "DATOS FICTICIOS", "PRUEBA", "AISLADA", "CALLE TEST 123", 200, new string('a', 64))
                    : new(false, null, null, null, null, 200, new string('b', 64));
            }
        }

        private sealed class ProveedorCancelado(CancellationTokenSource cancelacion) : IReniecProveedor
        {
            public Task<RespuestaReniec> ConsultarAsync(string dni, CancellationToken cancellationToken)
            {
                cancelacion.Cancel();
                return Task.FromCanceled<RespuestaReniec>(cancellationToken);
            }
        }

        private sealed class ApiAislada(PostgresAislado postgres) : WebApplicationFactory<Program>
        {
            public ProveedorFicticio Proveedor { get; } = new();
            private readonly byte[] clave = RandomNumberGenerator.GetBytes(64);
            public string Token(string cuentaId, string personaId, bool expirado = false, string audiencia = "Pruebas", bool permiso = true)
            {
                List<Claim> claims = [new("sub", cuentaId), new("persona_id", personaId), new("metodo_acceso", "dni_direccion")];
                if (permiso) claims.Add(new("permiso", "identidad.consultar-dni"));
                var ahora = DateTime.UtcNow;
                var token = new JwtSecurityToken("Pruebas", audiencia, claims, ahora.AddHours(-1),
                    expirado ? ahora.AddMinutes(-1) : ahora.AddMinutes(15),
                    new SigningCredentials(new SymmetricSecurityKey(clave), SecurityAlgorithms.HmacSha256));
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(servicios =>
                {
                    servicios.RemoveAll<DivorciosDbContext>();
                    servicios.AddScoped(_ => postgres.CrearContexto());
                    servicios.AddSingleton<IReniecProveedor>(Proveedor);
                    servicios.Configure<ReniecOpciones>(x => x.Habilitado = false);
                    servicios.Configure<AccesoCiudadanoOpciones>(x => x.HabilitarDniDireccion = true);
                    servicios.Configure<InformacionPreregistroOpciones>(x => { x.Formatos.Clear(); x.MimePermitidos.Clear(); x.MaximoBytes = null; });
                    servicios.Configure<JwtCiudadanoOpciones>(x => { x.ClaveBase64 = Convert.ToBase64String(clave); x.Emisor = "Pruebas"; x.Audiencia = "Pruebas"; });
                    servicios.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, x =>
                    {
                        x.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(clave);
                        x.TokenValidationParameters.ValidIssuer = "Pruebas";
                        x.TokenValidationParameters.ValidAudience = "Pruebas";
                    });
                });
            }
        }
    }
}
