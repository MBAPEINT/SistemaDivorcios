using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Divorcios.Dominio.Entidades;
using Divorcios.Negocio.DTOs.Preregistro;
using Divorcios.Negocio.Excepciones;
using Divorcios.Negocio.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static Divorcios.Pruebas.PaqueteBPruebas;

namespace Divorcios.Pruebas
{
    [Collection("PostgreSQL aislado")]
    public sealed class PaqueteCPruebas(PostgresAislado postgres)
    {
        private readonly PaqueteBPruebas apoyo = new(postgres);

        internal static CrearVersionPreregistroDto DatosVersion(string? baseId = null) => new()
        {
            VersionBaseId = baseId, MotivoCambio = baseId is null ? null : "Corrección de prueba aislada",
            Encuesta = new()
            {
                FechaMatrimonio = new(2020, 5, 10), MatrimonioEnPorvenir = true, UltimoDomicilioConyugalPorvenir = false,
                TieneHijos = true, CantidadHijosMenores = 0, CantidadHijosMayores = 1,
                TieneHijosMayoresSituacionEspecial = null, TieneBienes = false, TieneAcuerdoBienes = false,
                RequiereRepresentacionA = null, RequiereRepresentacionB = null
            },
            Contactos = [new() { PosicionCodigo = "A", Celular = "999888777", Correo = "prueba@example.invalid", Direccion = "DOMICILIO FICTICIO A" },
                new() { PosicionCodigo = "B" }]
        };

        private async Task<(PersonaPrueba[] Personas, PreregistroDetalleDto Pre)> Escenario()
        {
            var personas = await apoyo.Personas(cuentaSegundo: true);
            using var servicios = apoyo.Servicios();
            using var scope = servicios.CreateScope();
            var pre = await scope.ServiceProvider.GetRequiredService<IPreregistrosServicio>().CrearAsync(Actor(personas[0]), PaqueteBPruebas.Datos(personas), default);
            return (personas, pre);
        }

        [Fact]
        public async Task RespuestasNullYContactosSeConservanPorVersionInclusoConRelojFijo()
        {
            var (personas, pre) = await Escenario();
            using var servicios = apoyo.Servicios(reloj: new RelojFijo());
            using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>();
            var actor = Actor(personas[0]);
            Assert.Empty(await servicio.ListarAsync(actor, pre.PreregistroId, default));
            var uno = await servicio.CrearAsync(actor, pre.PreregistroId, DatosVersion(), default);
            Assert.Null(uno.Encuesta.TieneHijosMayoresSituacionEspecial);
            Assert.Null(uno.Encuesta.RequiereRepresentacionA);
            Assert.Null(uno.Encuesta.RequiereRepresentacionB);
            Assert.True(uno.Editable);
            Assert.Equal("PENDIENTE_CONFIGURACION", uno.EstadoGeneracionRequisitosCodigo);
            var requisitos = await servicio.ObtenerRequisitosAsync(actor, pre.PreregistroId, uno.PreregistroVersionId, default);
            Assert.Empty(requisitos.Items);
            Assert.NotEmpty(requisitos.PendientesConfiguracion);
            Assert.Equal("PENDIENTE_CONFIGURACION", requisitos.EstadoGeneracionCodigo);

            var datosDos = DatosVersion(uno.PreregistroVersionId);
            datosDos.Encuesta.RequiereRepresentacionA = true;
            datosDos.Encuesta.RequiereRepresentacionB = false;
            datosDos.Encuesta.TieneHijosMayoresSituacionEspecial = false;
            datosDos.Contactos[0].Celular = "999888666";
            var dos = await servicio.CrearAsync(actor, pre.PreregistroId, datosDos, default);
            var datosTres = DatosVersion(dos.PreregistroVersionId);
            datosTres.Contactos[0].Celular = null; datosTres.Contactos[0].Correo = null;
            var tres = await servicio.CrearAsync(actor, pre.PreregistroId, datosTres, default);
            Assert.True(uno.CreadoEn < dos.CreadoEn && dos.CreadoEn < tres.CreadoEn);
            var viejo = await servicio.ObtenerAsync(actor, pre.PreregistroId, uno.PreregistroVersionId, default);
            var intermedio = await servicio.ObtenerAsync(actor, pre.PreregistroId, dos.PreregistroVersionId, default);
            Assert.Null(viejo.Encuesta.RequiereRepresentacionA);
            Assert.True(intermedio.Encuesta.RequiereRepresentacionA);
            Assert.False(intermedio.Encuesta.RequiereRepresentacionB);
            Assert.Equal("999888777", viejo.Contactos[0].Celular);
            Assert.Equal("999888666", intermedio.Contactos[0].Celular);
            Assert.Equal("prueba@example.invalid", intermedio.Contactos[0].Correo);
            Assert.Null(tres.Contactos[0].Celular); Assert.Null(tres.Contactos[0].Correo);
            Assert.False(viejo.Editable);
            var lista = await servicio.ListarAsync(actor, pre.PreregistroId, default);
            Assert.Equal(new[] { 3, 2, 1 }, lista.Select(x => x.NumeroVersion));
            Assert.All(lista, x => Assert.False(x.Enviada));
            Assert.Single(lista, x => x.EsVersionTrabajo);

            await using var db = postgres.CrearContexto();
            var contactos = await db.ExpedientesContactosHistorial.Where(x => x.ExpedienteConyuge.ExpedienteId == long.Parse(pre.ExpedienteId)).ToListAsync();
            Assert.Equal(4, contactos.Count); // Dos celulares, un correo y una dirección sin duplicación.
            Assert.Single(contactos, x => x.TipoContactoCodigo == "DIRECCION");
            Assert.Equal(long.Parse(uno.PreregistroVersionId), contactos.Single(x => x.TipoContactoCodigo == "DIRECCION").PreregistroVersionOrigenId);
            Assert.Single(contactos, x => x.VigenteHasta is null);
            var auditorias = await db.RegistrosAuditoria.Where(x => x.ExpedienteId == long.Parse(pre.ExpedienteId) && x.AccionCodigo == "PREREGISTRO_VERSION_CREADA").ToListAsync();
            Assert.Equal(3, auditorias.Count);
            Assert.All(auditorias, x => Assert.Equal(personas[0].CuentaId, x.CuentaCiudadanaId));
            Assert.False(db.Database.HasPendingModelChanges());
        }

        [Theory]
        [InlineData("BOOL_AUSENTE")]
        [InlineData("CANTIDAD_AUSENTE")]
        [InlineData("SIN_HIJOS")]
        [InlineData("ESPECIAL_SIN_MAYORES")]
        [InlineData("SIN_BIENES")]
        [InlineData("CONTACTOS_DUPLICADOS")]
        [InlineData("CORREO_INVALIDO")]
        public async Task DatosIncoherentesNoPersistenVersion(string escenario)
        {
            var (personas, pre) = await Escenario();
            var datos = DatosVersion();
            switch (escenario)
            {
                case "BOOL_AUSENTE": datos.Encuesta.MatrimonioEnPorvenir = null; break;
                case "CANTIDAD_AUSENTE": datos.Encuesta.CantidadHijosMenores = null; break;
                case "SIN_HIJOS": datos.Encuesta.TieneHijos = false; break;
                case "ESPECIAL_SIN_MAYORES": datos.Encuesta.CantidadHijosMayores = 0; datos.Encuesta.TieneHijosMayoresSituacionEspecial = true; break;
                case "SIN_BIENES": datos.Encuesta.TieneAcuerdoBienes = true; break;
                case "CONTACTOS_DUPLICADOS": datos.Contactos[1].PosicionCodigo = "A"; break;
                case "CORREO_INVALIDO": datos.Contactos[0].Correo = "no-es-correo"; break;
            }
            using var servicios = apoyo.Servicios();
            using var scope = servicios.CreateScope();
            await Assert.ThrowsAsync<ValidationException>(() => scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>()
                .CrearAsync(Actor(personas[0]), pre.PreregistroId, datos, default));
            await using var db = postgres.CrearContexto();
            Assert.False(await db.PreregistrosVersiones.AnyAsync(x => x.PreregistroId == long.Parse(pre.PreregistroId)));
        }

        [Fact]
        public async Task SoloIniciadorEscribeAmbosLeenYVersionDeOtroPreEs404()
        {
            var (personas, pre) = await Escenario();
            using var servicios = apoyo.Servicios();
            using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>();
            await Assert.ThrowsAsync<AccesoDenegadoNegocioException>(() => servicio.CrearAsync(Actor(personas[1]), pre.PreregistroId, DatosVersion(), default));
            var uno = await servicio.CrearAsync(Actor(personas[0]), pre.PreregistroId, DatosVersion(), default);
            var segundo = await servicio.ObtenerAsync(Actor(personas[1]), pre.PreregistroId, uno.PreregistroVersionId, default);
            Assert.False(segundo.Editable); Assert.Equal(uno.Encuesta, segundo.Encuesta);
            Assert.Single(await servicio.ListarAsync(Actor(personas[1]), pre.PreregistroId, default));
            await Assert.ThrowsAsync<RecursoNoEncontradoNegocioException>(() => servicio.ListarAsync(Actor(personas[2]), pre.PreregistroId, default));
            var otroPre = await scope.ServiceProvider.GetRequiredService<IPreregistrosServicio>().CrearAsync(Actor(personas[0]), PaqueteBPruebas.Datos(personas), default);
            await Assert.ThrowsAsync<RecursoNoEncontradoNegocioException>(() => servicio.ObtenerAsync(Actor(personas[0]), otroPre.PreregistroId, uno.PreregistroVersionId, default));
            await Assert.ThrowsAsync<RecursoNoEncontradoNegocioException>(() => servicio.ObtenerRequisitosAsync(Actor(personas[0]), otroPre.PreregistroId, uno.PreregistroVersionId, default));
        }

        [Fact]
        public async Task VersionBaseYMotivoImpidenGuardadosPerdidos()
        {
            var (personas, pre) = await Escenario();
            using var servicios = apoyo.Servicios();
            using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>();
            var uno = await servicio.CrearAsync(Actor(personas[0]), pre.PreregistroId, DatosVersion(), default);
            var sinMotivo = DatosVersion(uno.PreregistroVersionId); sinMotivo.MotivoCambio = " ";
            await Assert.ThrowsAsync<ValidationException>(() => servicio.CrearAsync(Actor(personas[0]), pre.PreregistroId, sinMotivo, default));
            await Assert.ThrowsAsync<ConflictoNegocioException>(() => servicio.CrearAsync(Actor(personas[0]), pre.PreregistroId, DatosVersion(), default));
            async Task<bool> Guardar()
            {
                using var concurrente = servicios.CreateScope();
                try
                {
                    await concurrente.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>()
                        .CrearAsync(Actor(personas[0]), pre.PreregistroId, DatosVersion(uno.PreregistroVersionId), default);
                    return true;
                }
                catch (ConflictoNegocioException) { return false; }
            }
            var resultados = await Task.WhenAll(Guardar(), Guardar());
            Assert.Single(resultados, x => x);
            Assert.Single(resultados, x => !x);
            Assert.Equal(2, (await servicio.ListarAsync(Actor(personas[0]), pre.PreregistroId, default)).Count);
        }

        [Fact]
        public async Task FalloAuditoriaRevierteVersionCierreDeVigenciasYContactosNuevos()
        {
            var (personas, pre) = await Escenario();
            PreregistroVersionDto uno;
            using (var servicios = apoyo.Servicios())
            using (var scope = servicios.CreateScope())
                uno = await scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>()
                    .CrearAsync(Actor(personas[0]), pre.PreregistroId, DatosVersion(), default);
            using (var servicios = apoyo.Servicios(new FallarAuditoria()))
            using (var scope = servicios.CreateScope())
            {
                var datos = DatosVersion(uno.PreregistroVersionId); datos.Contactos[0].Celular = "999888111";
                await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>()
                    .CrearAsync(Actor(personas[0]), pre.PreregistroId, datos, default));
            }
            await using var db = postgres.CrearContexto();
            Assert.Single(await db.PreregistrosVersiones.Where(x => x.PreregistroId == long.Parse(pre.PreregistroId)).ToListAsync());
            var contactos = await db.ExpedientesContactosHistorial.Where(x => x.ExpedienteConyuge.ExpedienteId == long.Parse(pre.ExpedienteId)).ToListAsync();
            Assert.Equal(3, contactos.Count); Assert.All(contactos, x => Assert.Null(x.VigenteHasta));
            Assert.Equal("999888777", contactos.Single(x => x.TipoContactoCodigo == "CELULAR").Valor);
        }

        [Theory]
        [InlineData("ENVIADO")]
        [InlineData("APROBADO")]
        [InlineData("CANCELADO")]
        [InlineData("CERRADO")]
        [InlineData("REVISION_ABIERTA")]
        public async Task EstadosNoEditablesRechazanNuevaEncuesta(string escenario)
        {
            var (personas, pre) = await Escenario();
            using var servicios = apoyo.Servicios();
            using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>();
            var uno = await servicio.CrearAsync(Actor(personas[0]), pre.PreregistroId, DatosVersion(), default);
            await using var db = postgres.CrearContexto();
            var registro = await db.Preregistros.Include(x => x.Expediente).SingleAsync(x => x.PreregistroId == long.Parse(pre.PreregistroId));
            if (escenario == "CERRADO") registro.Expediente.CerradoEn = DateTime.UtcNow;
            else if (escenario == "REVISION_ABIERTA")
                db.RevisionesPrerregistro.Add(new() { PreregistroVersionId = long.Parse(uno.PreregistroVersionId), NumeroRevision = 1,
                    RevisadoPorUsuario = Usuario(), IniciadaEn = uno.CreadoEn });
            else
            {
                registro.EstadoCodigo = escenario;
                if (escenario is "ENVIADO" or "APROBADO") registro.EnviadoEn = uno.CreadoEn;
                if (escenario == "APROBADO") registro.AprobadoEn = uno.CreadoEn;
            }
            await db.SaveChangesAsync();
            await Assert.ThrowsAsync<ConflictoNegocioException>(() => servicio.CrearAsync(Actor(personas[0]), pre.PreregistroId, DatosVersion(uno.PreregistroVersionId), default));
            Assert.False((await servicio.ObtenerAsync(Actor(personas[0]), pre.PreregistroId, uno.PreregistroVersionId, default)).Editable);
        }

        [Fact]
        public async Task ObservacionCorreccionYReutilizacionNoMuevenOrigenPresentacionesNiEvaluaciones()
        {
            var (personas, pre) = await Escenario();
            using var servicios = apoyo.Servicios();
            using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>();
            var uno = await servicio.CrearAsync(Actor(personas[0]), pre.PreregistroId, DatosVersion(), default);
            await using var db = postgres.CrearContexto();
            var catalogo = new RequisitoCatalogo { Codigo = "FICTICIO-" + Guid.NewGuid().ToString("N")[..8], Nombre = "REQUISITO DE PRUEBA SIN VALIDEZ OFICIAL" };
            var tipo = new TipoDocumento { Codigo = "FICTICIO-" + Guid.NewGuid().ToString("N")[..8], Nombre = "ARCHIVO DE PRUEBA", OrigenCodigo = "CIUDADANO" };
            var requisito = new PreregistroRequisito { PreregistroVersionId = long.Parse(uno.PreregistroVersionId), RequisitoCatalogo = catalogo };
            Documento Documento() => new() { ExpedienteId = long.Parse(pre.ExpedienteId), TipoDocumento = tipo, PreregistroRequisito = requisito,
                Titulo = "EVIDENCIA FICTICIA", EtapaCodigo = "PRERREGISTRO", CreadoPorCuentaId = personas[0].CuentaId };
            DocumentoVersion Archivo(Documento documento, int numero) => new() { Documento = documento, NumeroVersion = numero,
                NombreArchivo = "archivo-ficticio-" + numero + ".pdf", MimeType = "application/pdf", TamanoBytes = 42,
                Sha256 = new string(numero == 1 ? 'a' : 'b', 64), AlmacenamientoClave = "pruebas/" + Guid.NewGuid(),
                CargadoPorCuentaId = personas[0].CuentaId, MotivoCambio = numero == 1 ? null : "CORRECCIÓN FICTICIA" };
            var documento = Documento(); var original = Archivo(documento, 1); var otro = Archivo(Documento(), 1);
            requisito.ArchivosPresentados.Add(new() { DocumentoVersion = original });
            requisito.ArchivosPresentados.Add(new() { DocumentoVersion = otro });
            var revision = new RevisionPreregistro { PreregistroVersionId = long.Parse(uno.PreregistroVersionId), NumeroRevision = 1,
                RevisadoPorUsuario = Usuario(), ResultadoCodigo = "OBSERVADO", IniciadaEn = uno.CreadoEn,
                FinalizadaEn = uno.CreadoEn.AddTicks(10), ComentarioGeneral = "CORREGIR ARCHIVO FICTICIO" };
            var detalle = new RevisionDetalle { RevisionPreregistro = revision, PreregistroRequisito = requisito,
                ResultadoCodigo = "OBSERVADO", Observacion = "OBSERVACIÓN DE PRUEBA" };
            detalle.DocumentosEvaluados.Add(new() { DocumentoVersion = original });
            detalle.DocumentosEvaluados.Add(new() { DocumentoVersion = otro });
            db.RevisionesDetalle.Add(detalle);
            var registro = await db.Preregistros.SingleAsync(x => x.PreregistroId == long.Parse(pre.PreregistroId));
            registro.EstadoCodigo = "OBSERVADO"; registro.EnviadoEn = uno.CreadoEn;
            await db.SaveChangesAsync();

            var dos = await servicio.CrearAsync(Actor(personas[0]), pre.PreregistroId, DatosVersion(uno.PreregistroVersionId), default);
            Assert.Empty((await servicio.ObtenerRequisitosAsync(Actor(personas[0]), pre.PreregistroId, dos.PreregistroVersionId, default)).Items);
            // Fixtures sólo en PostgreSQL temporal: simulan D03/D02 futuros, sin afirmar una matriz oficial.
            var requisitoDos = new PreregistroRequisito { PreregistroVersionId = long.Parse(dos.PreregistroVersionId), RequisitoCatalogo = catalogo };
            requisitoDos.ArchivosPresentados.Add(new() { DocumentoVersion = original });
            requisitoDos.ArchivosPresentados.Add(new() { DocumentoVersion = otro });
            db.PreregistrosRequisitos.Add(requisitoDos);
            var corregido = Archivo(documento, 2); db.DocumentosVersiones.Add(corregido);
            catalogo.Activo = false; tipo.Activo = false;
            await db.SaveChangesAsync();
            var historico = Assert.Single((await servicio.ObtenerRequisitosAsync(Actor(personas[0]), pre.PreregistroId, uno.PreregistroVersionId, default)).Items);
            var reutilizado = Assert.Single((await servicio.ObtenerRequisitosAsync(Actor(personas[0]), pre.PreregistroId, dos.PreregistroVersionId, default)).Items);
            Assert.Equal(2, historico.ArchivosPresentados.Count);
            Assert.Equal(historico.ArchivosPresentados, reutilizado.ArchivosPresentados);
            Assert.DoesNotContain(historico.ArchivosPresentados, x => x.DocumentoVersionId == corregido.DocumentoVersionId.ToString());
            Assert.Equal(requisito.PreregistroRequisitoId, documento.PreregistroRequisitoId);
            Assert.All(await db.RevisionesDetalleDocumentos.Where(x => x.RevisionDetalleId == detalle.RevisionDetalleId).ToListAsync(),
                x => Assert.NotEqual(corregido.DocumentoVersionId, x.DocumentoVersionId));
            Assert.False((await servicio.ObtenerAsync(Actor(personas[0]), pre.PreregistroId, uno.PreregistroVersionId, default)).Editable);
            var lista = await servicio.ListarAsync(Actor(personas[0]), pre.PreregistroId, default);
            Assert.False(lista[0].Enviada); Assert.True(lista[1].Enviada);
            Assert.Equal("OBSERVADO", Assert.Single(lista[1].Revisiones).ResultadoCodigo);
        }

        [Fact]
        public async Task ApiPublicaContrato201Location400403404YSwaggerProtegido()
        {
            var (personas, pre) = await Escenario();
            using var fabrica = new ApiAislada(postgres);
            using var cliente = fabrica.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
            var ruta = "/api/preregistros/" + pre.PreregistroId + "/versiones";
            Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync(ruta)).StatusCode);
            async Task Login(int indice)
            {
                var respuesta = await cliente.PostAsJsonAsync("/api/acceso-ciudadano/sesion", new { dni = personas[indice].Dni, sufijoDireccion = "123" });
                Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    (await respuesta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString());
            }
            await Login(0);
            var datos = DatosVersion();
            datos.Encuesta.TieneBienes = null;
            Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PostAsJsonAsync(ruta, datos)).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PostAsJsonAsync(ruta, new { encuesta = DatosVersion().Encuesta,
                contactos = DatosVersion().Contactos, creadoPorCuentaId = personas[2].CuentaId })).StatusCode);
            var creada = await cliente.PostAsJsonAsync(ruta, DatosVersion());
            Assert.Equal(HttpStatusCode.Created, creada.StatusCode);
            Assert.Equal("no-store", creada.Headers.CacheControl!.ToString());
            var version = await creada.Content.ReadFromJsonAsync<PreregistroVersionDto>();
            Assert.EndsWith(ruta + "/" + version!.PreregistroVersionId, creada.Headers.Location!.ToString());
            Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync(creada.Headers.Location)).StatusCode);
            var req = await cliente.GetFromJsonAsync<RequisitosVersionPreregistroDto>(ruta + "/" + version.PreregistroVersionId + "/requisitos");
            Assert.Empty(req!.Items); Assert.NotEmpty(req.PendientesConfiguracion);
            Assert.Equal(HttpStatusCode.Conflict, (await cliente.PostAsJsonAsync(ruta, DatosVersion())).StatusCode);
            await Login(1);
            Assert.Equal(HttpStatusCode.Forbidden, (await cliente.PostAsJsonAsync(ruta, DatosVersion(version.PreregistroVersionId))).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync(creada.Headers.Location)).StatusCode);
            await Login(2);
            Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync(creada.Headers.Location)).StatusCode);
            var swagger = await cliente.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
            foreach (var (path, method) in new[] { ("/api/preregistros/{preregistroId}/versiones", "post"),
                ("/api/preregistros/{preregistroId}/versiones", "get"), ("/api/preregistros/{preregistroId}/versiones/{versionId}", "get"),
                ("/api/preregistros/{preregistroId}/versiones/{versionId}/requisitos", "get") })
                Assert.True(swagger.GetProperty("paths").GetProperty(path).GetProperty(method).TryGetProperty("security", out _));
            Assert.Equal(0, fabrica.Proveedor.Llamadas);
        }

        [Fact]
        public async Task HistoriaSinEventoNoInventaEnvioNiGeneracionAcreditada()
        {
            var (personas, pre) = await Escenario();
            await using var db = postgres.CrearContexto();
            var version = new PreregistroVersion { PreregistroId = long.Parse(pre.PreregistroId), NumeroVersion = 1,
                FechaMatrimonio = new(2020, 1, 1), CreadoPorCuentaId = personas[0].CuentaId!.Value, CreadoEn = DateTime.UtcNow };
            db.PreregistrosVersiones.Add(version); await db.SaveChangesAsync();
            using var servicios = apoyo.Servicios();
            using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>();
            var historia = Assert.Single(await servicio.ListarAsync(Actor(personas[0]), pre.PreregistroId, default));
            Assert.Null(historia.Enviada);
            Assert.Equal("SIN_ACREDITAR", historia.EstadoGeneracionRequisitosCodigo);
            db.RegistrosAuditoria.Add(new() { ActorTipoCodigo = "CUENTA_CIUDADANA", CuentaCiudadanaId = personas[0].CuentaId,
                ExpedienteId = long.Parse(pre.ExpedienteId), AccionCodigo = "PREREGISTRO_VERSION_CREADA",
                RecursoCodigo = "PREREGISTRO_VERSION", RecursoId = version.PreregistroVersionId.ToString(), DetalleJson = "[]" });
            await db.SaveChangesAsync();
            var requisitos = await servicio.ObtenerRequisitosAsync(Actor(personas[0]), pre.PreregistroId, version.PreregistroVersionId.ToString(), default);
            Assert.Equal("SIN_ACREDITAR", requisitos.EstadoGeneracionCodigo);
            Assert.Empty(requisitos.Items); Assert.NotEmpty(requisitos.PendientesConfiguracion);
        }

        private static UsuarioInterno Usuario() => new() { Login = "prueba-c-" + Guid.NewGuid().ToString("N"),
            NombreVisible = "REVISOR FICTICIO", PasswordHash = "HASH FICTICIO SIN LOGIN REAL", RolCodigo = "ABOGADA" };
        private sealed class RelojFijo : TimeProvider
        {
            private readonly DateTimeOffset fecha = DateTimeOffset.UtcNow;
            public override DateTimeOffset GetUtcNow() => fecha;
        }
    }
}
