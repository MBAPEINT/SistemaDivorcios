using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Divorcios.Api.Seguridad;
using Divorcios.Datos.Contexto;
using Divorcios.Datos.Interfaces;
using Divorcios.Datos.Resultados;
using Divorcios.Datos.Opciones;
using Divorcios.Dominio.Entidades;
using Divorcios.Negocio.DTOs.Preregistro;
using Divorcios.Negocio.Excepciones;
using Divorcios.Negocio.Extensiones;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Opciones;
using Divorcios.Negocio.Resultados;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace Divorcios.Pruebas
{
    [Collection("PostgreSQL aislado")]
    public sealed class PaqueteBPruebas(PostgresAislado postgres)
    {
        private static int numeroPersona = 55000000;

        [Theory]
        [InlineData("A")]
        [InlineData("B")]
        public async Task CreacionAtomicaReutilizaPersonasSinEncuestaCuentaDelSegundoNiNumeroOficial(string posicionIniciador)
        {
            var personas = await Personas();
            using var servicios = Servicios();
            using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IPreregistrosServicio>();
            var datos = Datos(personas);
            if (posicionIniciador == "B") { datos.Conyuges[0].PosicionCodigo = "B"; datos.Conyuges[1].PosicionCodigo = "A"; }
            var detalle = await servicio.CrearAsync(Actor(personas[0]), datos, default);
            Assert.Equal("BORRADOR", detalle.EstadoCodigo);
            Assert.Null(detalle.NumeroExpediente);
            Assert.Null(detalle.VersionTrabajoId);
            Assert.Equal(2, detalle.Conyuges.Count);
            Assert.Single(detalle.Conyuges, x => x.EsIniciador);
            Assert.Equal(posicionIniciador, detalle.Conyuges.Single(x => x.EsIniciador).PosicionCodigo);
            Assert.True(detalle.SoyIniciador);
            Assert.Contains("CREAR_VERSION", detalle.AccionesDisponibles);
            Assert.DoesNotContain("CREAR_VERSION", detalle.AccionesPendientesImplementacion);
            await using var db = postgres.CrearContexto();
            var expId = long.Parse(detalle.ExpedienteId);
            var expediente = await db.Expedientes.SingleAsync(x => x.ExpedienteId == expId);
            Assert.Equal(personas[0].CuentaId, expediente.CreadoPorCuentaId);
            Assert.Null(expediente.FechaIngresoMesaPartes);
            Assert.Null(expediente.OficializadoEn);
            Assert.False(await db.CuentasCiudadanas.AnyAsync(x => x.PersonaId == personas[1].PersonaId));
            Assert.False(await db.PreregistrosVersiones.AnyAsync(x => x.PreregistroId == long.Parse(detalle.PreregistroId)));
            var auditoria = await db.RegistrosAuditoria.SingleAsync(x => x.ExpedienteId == expId);
            Assert.Equal("PREREGISTRO_CREADO", auditoria.AccionCodigo);
            Assert.Equal(personas[0].CuentaId, auditoria.CuentaCiudadanaId);
            Assert.Equal(detalle.PreregistroId, auditoria.RecursoId);
            Assert.Equal(3, await db.Personas.CountAsync(x => personas.Select(p => p.PersonaId).Contains(x.PersonaId)));
        }

        [Theory]
        [InlineData("MISMA_PERSONA")]
        [InlineData("MISMA_POSICION")]
        [InlineData("DOS_INICIADORES")]
        [InlineData("SIN_INICIADOR")]
        [InlineData("INICIADOR_AJENO")]
        [InlineData("PERSONA_INEXISTENTE")]
        [InlineData("RESPUESTA_PENDIENTE")]
        public async Task DatosInvalidosNoCreanExpediente(string escenario)
        {
            var personas = await Personas();
            var datos = Datos(personas);
            switch (escenario)
            {
                case "MISMA_PERSONA": datos.Conyuges[1].PersonaId = datos.Conyuges[0].PersonaId; break;
                case "MISMA_POSICION": datos.Conyuges[1].PosicionCodigo = "A"; break;
                case "DOS_INICIADORES": datos.Conyuges[1].EsIniciador = true; break;
                case "SIN_INICIADOR": datos.Conyuges[0].EsIniciador = false; break;
                case "INICIADOR_AJENO": datos.Conyuges[0].EsIniciador = false; datos.Conyuges[1].EsIniciador = true; break;
                case "PERSONA_INEXISTENTE": datos.Conyuges[1].PersonaId = long.MaxValue.ToString(); break;
                case "RESPUESTA_PENDIENTE": datos.Conyuges[0].EsIniciador = null; break;
            }
            await using var db = postgres.CrearContexto();
            var antes = await db.Expedientes.CountAsync();
            using var servicios = Servicios();
            using var scope = servicios.CreateScope();
            await Assert.ThrowsAsync<ValidationException>(() => scope.ServiceProvider.GetRequiredService<IPreregistrosServicio>()
                .CrearAsync(Actor(personas[0]), datos, default));
            Assert.Equal(antes, await db.Expedientes.CountAsync());
        }

        [Fact]
        public async Task PersonaSinVerificacionYActorSuplantadoSonRechazados()
        {
            var personas = await Personas();
            await using var db = postgres.CrearContexto();
            var segunda = await db.Personas.SingleAsync(x => x.PersonaId == personas[1].PersonaId);
            segunda.VerificadoReniec = false; segunda.VerificadoReniecEn = null;
            await db.SaveChangesAsync();
            using var servicios = Servicios();
            using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IPreregistrosServicio>();
            await Assert.ThrowsAsync<ValidationException>(() => servicio.CrearAsync(Actor(personas[0]), Datos(personas), default));
            await Assert.ThrowsAsync<AccesoCiudadanoRechazadoException>(() => servicio.ListarAsync(
                new(personas[0].CuentaId!.Value, personas[2].PersonaId, Guid.NewGuid()), new(), default));
        }

        [Fact]
        public async Task AmbosParticipantesLeenElMismoPreYUnTerceroNoLoVe()
        {
            var personas = await Personas(cuentaSegundo: true);
            using var servicios = Servicios();
            using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IPreregistrosServicio>();
            var creado = await servicio.CrearAsync(Actor(personas[0]), Datos(personas), default);
            var segundo = await servicio.ObtenerAsync(Actor(personas[1]), creado.PreregistroId, default);
            Assert.Equal(creado.ExpedienteId, segundo.ExpedienteId);
            Assert.False(segundo.SoyIniciador);
            Assert.Empty(segundo.AccionesPendientesImplementacion);
            var listaSegundo = await servicio.ListarAsync(Actor(personas[1]), new(), default);
            Assert.Single(listaSegundo.Items);
            Assert.Equal(creado.PreregistroId, listaSegundo.Items[0].PreregistroId);
            Assert.Empty((await servicio.ListarAsync(Actor(personas[2]), new(), default)).Items);
            await Assert.ThrowsAsync<RecursoNoEncontradoNegocioException>(() => servicio.ObtenerAsync(Actor(personas[2]), creado.PreregistroId, default));
        }

        [Fact]
        public async Task PaginacionFiltroYRepeticionNoProhibenPreregistrosHistoricosPorDni()
        {
            var personas = await Personas();
            using var servicios = Servicios();
            using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IPreregistrosServicio>();
            var uno = await servicio.CrearAsync(Actor(personas[0]), Datos(personas), default);
            var dos = await servicio.CrearAsync(Actor(personas[0]), Datos(personas), default);
            Assert.NotEqual(uno.ExpedienteId, dos.ExpedienteId);
            Assert.NotEqual(uno.CodigoPreregistro, dos.CodigoPreregistro);
            var pagina = await servicio.ListarAsync(Actor(personas[0]), new() { TamanoPagina = 1 }, default);
            Assert.Equal(2, pagina.Total);
            Assert.Equal(dos.PreregistroId, Assert.Single(pagina.Items).PreregistroId);
            Assert.Equal(uno.PreregistroId, Assert.Single((await servicio.ListarAsync(Actor(personas[0]), new() { TamanoPagina = 1, Pagina = 2 }, default)).Items).PreregistroId);
            Assert.Empty((await servicio.ListarAsync(Actor(personas[0]), new() { EstadoCodigo = "CANCELADO" }, default)).Items);
            Assert.Empty((await servicio.ListarAsync(Actor(personas[0]), new() { Pagina = int.MaxValue }, default)).Items);
        }

        [Fact]
        public async Task FalloDeAuditoriaRevierteExpedienteParticipantesYPre()
        {
            var personas = await Personas();
            await using var db = postgres.CrearContexto();
            var antes = new[] { await db.Expedientes.CountAsync(), await db.ExpedientesConyuges.CountAsync(), await db.Preregistros.CountAsync() };
            using var servicios = Servicios(new FallarAuditoria());
            using var scope = servicios.CreateScope();
            await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<IPreregistrosServicio>()
                .CrearAsync(Actor(personas[0]), Datos(personas), default));
            Assert.Equal(antes, new[] { await db.Expedientes.CountAsync(), await db.ExpedientesConyuges.CountAsync(), await db.Preregistros.CountAsync() });
        }

        [Fact]
        public async Task ReferenciasHistoricasUsanAuditoriaYRevisionNuncaLaUltimaEncuesta()
        {
            var personas = await Personas();
            using var servicios = Servicios();
            using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IPreregistrosServicio>();
            var creado = await servicio.CrearAsync(Actor(personas[0]), Datos(personas), default);
            await using var db = postgres.CrearContexto();
            var preId = long.Parse(creado.PreregistroId);
            var pre = await db.Preregistros.SingleAsync(x => x.PreregistroId == preId);
            var enviado = pre.CreadoEn.AddMinutes(1);
            var aprobado = enviado.AddMinutes(2);
            var versionUno = new PreregistroVersion { PreregistroId = preId, NumeroVersion = 1, FechaMatrimonio = new(2020, 1, 1),
                CreadoPorCuentaId = personas[0].CuentaId!.Value, CreadoEn = pre.CreadoEn };
            var versionDos = new PreregistroVersion { PreregistroId = preId, NumeroVersion = 2, FechaMatrimonio = new(2020, 1, 1),
                CreadoPorCuentaId = personas[0].CuentaId!.Value, CreadoEn = aprobado.AddMinutes(1), MotivoCambio = "DATOS FICTICIOS DE HISTORIA" };
            var usuario = new UsuarioInterno { Login = "prueba-" + Guid.NewGuid().ToString("N"), NombreVisible = "REVISOR FICTICIO",
                PasswordHash = "HASH FICTICIO SIN LOGIN REAL", RolCodigo = "ABOGADA" };
            db.PreregistrosVersiones.AddRange(versionUno, versionDos);
            db.RevisionesPrerregistro.Add(new() { PreregistroVersion = versionUno, NumeroRevision = 1, RevisadoPorUsuario = usuario,
                ResultadoCodigo = "APROBADO", IniciadaEn = enviado.AddMinutes(1), FinalizadaEn = aprobado });
            pre.EnviadoEn = enviado; pre.AprobadoEn = aprobado; pre.EstadoCodigo = "APROBADO";
            await db.SaveChangesAsync();
            db.RegistrosAuditoria.Add(new() { ActorTipoCodigo = "CUENTA_CIUDADANA", CuentaCiudadanaId = personas[0].CuentaId,
                ExpedienteId = pre.ExpedienteId, AccionCodigo = "PREREGISTRO_ENVIADO", RecursoCodigo = "PREREGISTRO_VERSION",
                RecursoId = versionUno.PreregistroVersionId.ToString(), RegistradoEn = enviado });
            await db.SaveChangesAsync();
            var detalle = await servicio.ObtenerAsync(Actor(personas[0]), creado.PreregistroId, default);
            Assert.Equal(versionUno.PreregistroVersionId.ToString(), detalle.VersionEnviadaId);
            Assert.Equal(versionUno.PreregistroVersionId.ToString(), detalle.VersionAprobadaId);
            Assert.NotEqual(versionDos.PreregistroVersionId.ToString(), detalle.VersionAprobadaId);
            Assert.Null(detalle.VersionTrabajoId);
            Assert.Empty(detalle.ReferenciasPendientes);
        }

        [Fact]
        public async Task EnvioSinEvidenciaExactaConservaReferenciaPendienteYNoHabilitaEdicion()
        {
            var personas = await Personas();
            using var servicios = Servicios();
            using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IPreregistrosServicio>();
            var creado = await servicio.CrearAsync(Actor(personas[0]), Datos(personas), default);
            await using var db = postgres.CrearContexto();
            var id = long.Parse(creado.PreregistroId);
            var pre = await db.Preregistros.SingleAsync(x => x.PreregistroId == id);
            pre.EstadoCodigo = "ENVIADO"; pre.EnviadoEn = pre.CreadoEn.AddMinutes(1);
            await db.SaveChangesAsync();
            var detalle = await servicio.ObtenerAsync(Actor(personas[0]), creado.PreregistroId, default);
            Assert.Null(detalle.VersionEnviadaId);
            Assert.Contains("VERSION_ENVIADA", detalle.ReferenciasPendientes);
            Assert.Empty(detalle.AccionesPendientesImplementacion);
        }

        [Fact]
        public async Task ApiTiene201LocationLecturaPorSesion404AjenoY400CamposProtegidos()
        {
            var personas = await Personas();
            using var fabrica = new ApiAislada(postgres);
            using var cliente = fabrica.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
            Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/api/preregistros")).StatusCode);
            var login = await cliente.PostAsJsonAsync("/api/acceso-ciudadano/sesion", new { dni = personas[0].Dni, sufijoDireccion = "123" });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            var token = (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString();
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var creada = await cliente.PostAsJsonAsync("/api/preregistros", Datos(personas));
            Assert.Equal(HttpStatusCode.Created, creada.StatusCode);
            Assert.Equal("no-store", creada.Headers.CacheControl!.ToString());
            var pre = await creada.Content.ReadFromJsonAsync<JsonElement>();
            var id = pre.GetProperty("preregistroId").GetString()!;
            Assert.EndsWith("/api/preregistros/" + id, creada.Headers.Location!.ToString());
            Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync(creada.Headers.Location)).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await cliente.GetAsync("/api/preregistros/-1")).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await cliente.GetAsync("/api/preregistros?tamanoPagina=101")).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PostAsJsonAsync("/api/preregistros", new { conyuges = Datos(personas).Conyuges, cuentaCiudadanaId = personas[2].CuentaId })).StatusCode);
            var loginTercero = await cliente.PostAsJsonAsync("/api/acceso-ciudadano/sesion", new { dni = personas[2].Dni, sufijoDireccion = "123" });
            var tokenTercero = (await loginTercero.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString();
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenTercero);
            Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync("/api/preregistros/" + id)).StatusCode);
            var lista = await cliente.GetFromJsonAsync<JsonElement>("/api/preregistros");
            Assert.Empty(lista.GetProperty("items").EnumerateArray());
            Assert.Equal(0, fabrica.Proveedor.Llamadas);
            var swagger = await cliente.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
            Assert.True(swagger.GetProperty("paths").GetProperty("/api/preregistros").GetProperty("post").TryGetProperty("security", out _));
            Assert.True(swagger.GetProperty("paths").GetProperty("/api/preregistros/{preregistroId}").GetProperty("get").TryGetProperty("security", out _));
        }

        internal ServiceProvider Servicios(IInterceptor? interceptor = null, TimeProvider? reloj = null)
        {
            var servicios = new ServiceCollection();
            servicios.AddLogging();
            servicios.AgregarCapaNegocio(postgres.Conexion);
            servicios.RemoveAll<DivorciosDbContext>();
            servicios.AddScoped(_ => postgres.CrearContexto(interceptor));
            servicios.AddSingleton<IReniecProveedor>(new ProveedorProhibido());
            if (reloj is not null) servicios.AddSingleton(reloj);
            return servicios.BuildServiceProvider();
        }

        internal async Task<PersonaPrueba[]> Personas(bool cuentaSegundo = false)
        {
            await using var db = postgres.CrearContexto();
            List<(Persona Persona, CuentaCiudadana? Cuenta)> datos = [];
            var ahora = DateTime.UtcNow;
            for (var indice = 0; indice < 3; indice++)
            {
                var persona = new Persona { Dni = Interlocked.Increment(ref numeroPersona).ToString(), Nombres = "PERSONA FICTICIA",
                    ApellidoPaterno = "PAQUETE", ApellidoMaterno = "PRUEBAS", DireccionDni = "CALLE FICTICIA 123",
                    VerificadoReniec = true, VerificadoReniecEn = ahora, CreadoEn = ahora };
                var cuenta = indice != 1 || cuentaSegundo ? new CuentaCiudadana { Persona = persona, CreadoEn = ahora } : null;
                db.ConsultasReniec.Add(new() { Persona = persona, DniConsultado = persona.Dni, Prenombres = persona.Nombres,
                    ApellidoPaterno = persona.ApellidoPaterno, ApellidoMaterno = persona.ApellidoMaterno, Direccion = persona.DireccionDni,
                    ResultadoCodigo = "ENCONTRADO", OrigenCodigo = "IMPORTACION", ConsultadoEn = ahora, ExpiraEn = null });
                if (cuenta is not null) db.CuentasCiudadanas.Add(cuenta);
                datos.Add((persona, cuenta));
            }
            await db.SaveChangesAsync();
            return datos.Select(x => new PersonaPrueba(x.Persona.PersonaId, x.Cuenta?.CuentaCiudadanaId, x.Persona.Dni)).ToArray();
        }

        internal sealed record PersonaPrueba(long PersonaId, long? CuentaId, string Dni);
        internal static ActorCiudadano Actor(PersonaPrueba persona) => new(persona.CuentaId!.Value, persona.PersonaId, Guid.NewGuid());
        internal static CrearPreregistroDto Datos(PersonaPrueba[] personas) => new()
        {
            Conyuges = [new() { PersonaId = personas[0].PersonaId.ToString(), PosicionCodigo = "A", EsIniciador = true },
                new() { PersonaId = personas[1].PersonaId.ToString(), PosicionCodigo = "B", EsIniciador = false }]
        };
        internal sealed class FallarAuditoria : SaveChangesInterceptor
        {
            public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
                InterceptionResult<int> result, CancellationToken cancellationToken = default)
            {
                if (eventData.Context!.ChangeTracker.Entries<RegistroAuditoria>().Any(x => x.State == EntityState.Added))
                    throw new InvalidOperationException("Fallo de auditoría simulado exclusivamente en la prueba aislada.");
                return ValueTask.FromResult(result);
            }
        }
        internal sealed class ProveedorProhibido : IReniecProveedor
        {
            public int Llamadas { get; private set; }
            public Task<RespuestaReniec> ConsultarAsync(string dni, CancellationToken cancellationToken)
            {
                Llamadas++;
                throw new InvalidOperationException("El paquete B no debe llamar al proveedor externo de identidad.");
            }
        }
        internal sealed class ApiAislada(PostgresAislado postgres, Action<IServiceCollection>? configurar = null) : WebApplicationFactory<Program>
        {
            public ProveedorProhibido Proveedor { get; } = new();
            private readonly byte[] clave = RandomNumberGenerator.GetBytes(64);
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.UseEnvironment("Development");
                builder.ConfigureTestServices(servicios =>
                {
                    servicios.RemoveAll<DivorciosDbContext>(); servicios.AddScoped(_ => postgres.CrearContexto());
                    servicios.AddSingleton<IReniecProveedor>(Proveedor);
                    servicios.Configure<ReniecOpciones>(x => x.Habilitado = false);
                    servicios.Configure<Divorcios.Negocio.Opciones.MatrizRequisitosOpciones>(x => x.HabilitarGeneracionParcial = false);
                    servicios.Configure<AccesoCiudadanoOpciones>(x => x.HabilitarDniDireccion = true);
                    servicios.Configure<JwtCiudadanoOpciones>(x => { x.ClaveBase64 = Convert.ToBase64String(clave); x.Emisor = "Pruebas"; x.Audiencia = "Pruebas"; });
                    servicios.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, x =>
                    {
                        x.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(clave);
                        x.TokenValidationParameters.ValidIssuer = "Pruebas"; x.TokenValidationParameters.ValidAudience = "Pruebas";
                    });
                    configurar?.Invoke(servicios);
                });
            }
        }
    }
}
