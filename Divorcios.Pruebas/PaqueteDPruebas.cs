using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Divorcios.Datos.Almacenamiento;
using Divorcios.Datos.Contexto;
using Divorcios.Datos.Excepciones;
using Divorcios.Datos.Interfaces;
using Divorcios.Datos.Opciones;
using Divorcios.Dominio.Entidades;
using Divorcios.Negocio.DTOs.Preregistro;
using Divorcios.Negocio.Excepciones;
using Divorcios.Negocio.Extensiones;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Opciones;
using Divorcios.Negocio.Resultados;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using static Divorcios.Pruebas.PaqueteBPruebas;

namespace Divorcios.Pruebas
{
    [Collection("PostgreSQL aislado")]
    public sealed class PaqueteDPruebas(PostgresAislado postgres) : IDisposable
    {
        private readonly PaqueteBPruebas apoyo = new(postgres);
        private readonly string raiz = Path.Combine(Path.GetTempPath(), "divorcios-archivos-prueba-" + Guid.NewGuid().ToString("N"));
        internal static byte[] Pdf(string texto = "evidencia ficticia", bool sinPaginas = false)
        {
            // PDF mínimo independiente del parser usado por el almacenamiento: catálogo, página, stream y xref.
            var contenido = "% " + texto.Replace('\n', ' ').Replace('\r', ' ') + "\n";
            string[] objetos = ["<< /Type /Catalog /Pages 2 0 R >>", sinPaginas ? "<< /Type /Pages /Kids [] /Count 0 >>" : "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << >> /Contents 4 0 R >>",
                "<< /Length " + contenido.Length + " >>\nstream\n" + contenido + "endstream"];
            var pdf = new StringBuilder("%PDF-1.7\n");
            List<int> offsets = [];
            for (var i = 0; i < objetos.Length; i++)
            {
                offsets.Add(pdf.Length); pdf.Append(i + 1).Append(" 0 obj\n").Append(objetos[i]).Append("\nendobj\n");
            }
            var xref = pdf.Length;
            pdf.Append("xref\n0 5\n0000000000 65535 f \n");
            foreach (var offset in offsets) pdf.Append(offset.ToString("D10", System.Globalization.CultureInfo.InvariantCulture)).Append(" 00000 n \n");
            pdf.Append("trailer\n<< /Size 5 /Root 1 0 R >>\nstartxref\n").Append(xref).Append("\n%%EOF\n");
            return Encoding.ASCII.GetBytes(pdf.ToString());
        }
        private static ArchivoEntrada Entrada(byte[]? bytes = null, string nombre = "evidencia.pdf", long? declarado = null)
        {
            bytes ??= Pdf();
            return new(new MemoryStream(bytes), nombre, declarado ?? bytes.Length);
        }
        private void Configurar(IServiceCollection servicios, bool politica = true, bool correspondencia = true)
        {
            servicios.Configure<AlmacenamientoArchivosOpciones>(x => x.DirectorioRaiz = raiz);
            servicios.Configure<InformacionPreregistroOpciones>(x =>
            {
                x.MaximoBytes = politica ? 1024 : null;
                x.MimePermitidos = politica ? ["application/pdf"] : [];
            });
            servicios.Configure<ArchivosPreregistroOpciones>(x => x.TiposPorRequisito = correspondencia
                ? [new() { RequisitoCodigo = "FICTICIO-D", Confirmada = true, TiposDocumentoCodigos = ["FICTICIO-D"] }] : []);
        }
        private ServiceProvider Servicios(IInterceptor? interceptor = null, bool politica = true, bool correspondencia = true)
        {
            var servicios = new ServiceCollection();
            servicios.AddLogging(); servicios.AgregarCapaNegocio(postgres.Conexion);
            servicios.RemoveAll<DivorciosDbContext>(); servicios.AddScoped(_ => postgres.CrearContexto(interceptor));
            servicios.AddSingleton<IReniecProveedor>(new ProveedorProhibido());
            Configurar(servicios, politica, correspondencia);
            return servicios.BuildServiceProvider();
        }
        private sealed record Escenario(PersonaPrueba[] Personas, PreregistroDetalleDto Pre, PreregistroVersionDto Version,
            string RequisitoId, short TipoId, short CatalogoId);
        private async Task<Escenario> CrearEscenario()
        {
            var personas = await apoyo.Personas(cuentaSegundo: true);
            using var servicios = Servicios(); using var scope = servicios.CreateScope();
            var pre = await scope.ServiceProvider.GetRequiredService<IPreregistrosServicio>()
                .CrearAsync(Actor(personas[0]), PaqueteBPruebas.Datos(personas), default);
            var version = await scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>()
                .CrearAsync(Actor(personas[0]), pre.PreregistroId, PaqueteCPruebas.DatosVersion(), default);
            await using var db = postgres.CrearContexto();
            var catalogo = await db.RequisitosCatalogo.SingleOrDefaultAsync(x => x.Codigo == "FICTICIO-D")
                ?? new RequisitoCatalogo { Codigo = "FICTICIO-D", Nombre = "REQUISITO FICTICIO SIN VALIDEZ OFICIAL" };
            var tipo = await db.TiposDocumento.SingleOrDefaultAsync(x => x.Codigo == "FICTICIO-D")
                ?? new TipoDocumento { Codigo = "FICTICIO-D", Nombre = "ARCHIVO FICTICIO", OrigenCodigo = "CIUDADANO" };
            catalogo.Activo = true; tipo.Activo = true; tipo.OrigenCodigo = "CIUDADANO";
            if (tipo.TipoDocumentoId == 0) db.TiposDocumento.Add(tipo);
            var requisito = new PreregistroRequisito { PreregistroVersionId = long.Parse(version.PreregistroVersionId), RequisitoCatalogo = catalogo };
            db.PreregistrosRequisitos.Add(requisito); await db.SaveChangesAsync();
            return new(personas, pre, version, requisito.PreregistroRequisitoId.ToString(), tipo.TipoDocumentoId, catalogo.RequisitoCatalogoId);
        }
        private static Task<DocumentoVersionDto> Cargar(IArchivosPreregistroServicio servicio, Escenario e, ArchivoEntrada? entrada = null)
            => servicio.CargarAsync(Actor(e.Personas[0]), e.Pre.PreregistroId, e.Version.PreregistroVersionId,
                e.RequisitoId, e.TipoId, "ARCHIVO FICTICIO", entrada ?? Entrada(), default);
        private static Task<PreregistroRequisitoDto> Seleccionar(IArchivosPreregistroServicio servicio, Escenario e, params string[] ids)
            => servicio.SeleccionarAsync(Actor(e.Personas[0]), e.Pre.PreregistroId, e.Version.PreregistroVersionId, e.RequisitoId,
                new() { DocumentoVersionIds = ids.ToList() }, default);

        [Fact]
        public async Task CargaVariosArchivosConHashYOrigenDescargaExactaParaAmbosSinExponerClaves()
        {
            var e = await CrearEscenario(); using var servicios = Servicios(); using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>();
            var bytes = Pdf(); var uno = await Cargar(servicio, e, Entrada(bytes, "../../evidencia.pdf"));
            var dos = await Cargar(servicio, e, Entrada(Pdf("segundo")));
            Assert.Equal("evidencia.pdf", uno.NombreArchivo);
            Assert.Equal(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(), uno.Sha256);
            Assert.Equal(bytes.Length, uno.TamanoBytes);
            foreach (var persona in e.Personas.Take(2))
            {
                var descarga = await servicio.DescargarAsync(Actor(persona), e.Pre.PreregistroId, uno.DocumentoId, uno.DocumentoVersionId, default);
                await using var stream = descarga.Contenido; using var copia = new MemoryStream(); await stream.CopyToAsync(copia);
                Assert.Equal(bytes, copia.ToArray());
                var pagina = await servicio.ListarAsync(Actor(persona), e.Pre.PreregistroId, new(), default);
                Assert.Equal(2, pagina.Total); Assert.All(pagina.Items, x => Assert.Equal(e.RequisitoId, x.PreregistroRequisitoOrigenId));
                Assert.DoesNotContain(raiz, JsonSerializer.Serialize(pagina)); Assert.DoesNotContain("AlmacenamientoClave", JsonSerializer.Serialize(pagina));
            }
            await using var db = postgres.CrearContexto();
            var req = await db.PreregistrosRequisitos.SingleAsync(x => x.PreregistroRequisitoId == long.Parse(e.RequisitoId));
            Assert.Equal("CARGADO", req.EstadoCodigo);
            Assert.Equal(2, await db.PreregistrosRequisitosDocumentos.CountAsync(x => x.PreregistroRequisitoId == req.PreregistroRequisitoId));
            Assert.False(db.Database.HasPendingModelChanges());
            await Assert.ThrowsAsync<RecursoNoEncontradoNegocioException>(() => servicio.DescargarAsync(Actor(e.Personas[2]), e.Pre.PreregistroId, uno.DocumentoId, uno.DocumentoVersionId, default));
            await Assert.ThrowsAsync<RecursoNoEncontradoNegocioException>(() => servicio.DescargarAsync(Actor(e.Personas[0]), e.Pre.PreregistroId, dos.DocumentoId, uno.DocumentoVersionId, default));
            await Assert.ThrowsAsync<AccesoDenegadoNegocioException>(() => servicio.CargarAsync(Actor(e.Personas[1]), e.Pre.PreregistroId,
                e.Version.PreregistroVersionId, e.RequisitoId, e.TipoId, "FICTICIO", Entrada(), default));
        }

        [Fact]
        public async Task ReutilizarYCorregirConservaPresentacionesEvaluadasOrigenYBytesHistoricos()
        {
            var e = await CrearEscenario(); using var servicios = Servicios(); using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>();
            var encuestas = scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>();
            var uno = await Cargar(servicio, e); var otro = await Cargar(servicio, e, Entrada(Pdf("otro")));
            await using (var db = postgres.CrearContexto())
            {
                var revision = new RevisionPreregistro { PreregistroVersionId = long.Parse(e.Version.PreregistroVersionId), NumeroRevision = 1,
                    RevisadoPorUsuario = new() { Login = "prueba-d-" + Guid.NewGuid().ToString("N"), NombreVisible = "ABOGADA FICTICIA",
                        PasswordHash = "FICTICIO", RolCodigo = "ABOGADA" }, IniciadaEn = e.Version.CreadoEn,
                    FinalizadaEn = e.Version.CreadoEn.AddTicks(10), ResultadoCodigo = "OBSERVADO" };
                var detalle = new RevisionDetalle { RevisionPreregistro = revision, PreregistroRequisitoId = long.Parse(e.RequisitoId),
                    ResultadoCodigo = "OBSERVADO", Observacion = "CORREGIR ARCHIVO DE PRUEBA" };
                detalle.DocumentosEvaluados.Add(new() { DocumentoVersionId = long.Parse(uno.DocumentoVersionId) });
                detalle.DocumentosEvaluados.Add(new() { DocumentoVersionId = long.Parse(otro.DocumentoVersionId) });
                db.RevisionesDetalle.Add(detalle);
                var pre = await db.Preregistros.SingleAsync(x => x.PreregistroId == long.Parse(e.Pre.PreregistroId));
                pre.EstadoCodigo = "OBSERVADO"; pre.EnviadoEn = e.Version.CreadoEn;
                await db.SaveChangesAsync();
            }
            Assert.False((await encuestas.ObtenerAsync(Actor(e.Personas[0]), e.Pre.PreregistroId, e.Version.PreregistroVersionId, default)).Editable);
            Assert.Null((await scope.ServiceProvider.GetRequiredService<IPreregistrosServicio>().ObtenerAsync(Actor(e.Personas[0]), e.Pre.PreregistroId, default)).VersionTrabajoId);
            await Assert.ThrowsAsync<ConflictoNegocioException>(() => Seleccionar(servicio, e));
            var nueva = await encuestas.CrearAsync(Actor(e.Personas[0]), e.Pre.PreregistroId, PaqueteCPruebas.DatosVersion(e.Version.PreregistroVersionId), default);
            Assert.Empty((await encuestas.ObtenerRequisitosAsync(Actor(e.Personas[0]), e.Pre.PreregistroId, nueva.PreregistroVersionId, default)).Items);
            string requisitoNuevo;
            await using (var db = postgres.CrearContexto())
            {
                var requisito = new PreregistroRequisito { PreregistroVersionId = long.Parse(nueva.PreregistroVersionId), RequisitoCatalogoId = e.CatalogoId };
                db.PreregistrosRequisitos.Add(requisito); await db.SaveChangesAsync(); requisitoNuevo = requisito.PreregistroRequisitoId.ToString();
            }
            var trabajo = e with { Version = nueva, RequisitoId = requisitoNuevo };
            var reutilizado = await Seleccionar(servicio, trabajo, uno.DocumentoVersionId, otro.DocumentoVersionId);
            Assert.Equal(2, reutilizado.ArchivosPresentados.Count);
            var corregido = await servicio.CorregirAsync(Actor(e.Personas[0]), e.Pre.PreregistroId, nueva.PreregistroVersionId, requisitoNuevo,
                uno.DocumentoId, uno.DocumentoVersionId, "CORRECCIÓN FICTICIA", Entrada(Pdf("correccion")), default);
            Assert.Equal(uno.DocumentoId, corregido.DocumentoId); Assert.Equal(2, corregido.NumeroVersion); Assert.NotEqual(uno.Sha256, corregido.Sha256);
            var viejo = Assert.Single((await encuestas.ObtenerRequisitosAsync(Actor(e.Personas[1]), e.Pre.PreregistroId, e.Version.PreregistroVersionId, default)).Items);
            Assert.Equal(new[] { uno.DocumentoVersionId, otro.DocumentoVersionId }.Order(), viejo.ArchivosPresentados.Select(x => x.DocumentoVersionId).Order());
            var actual = await Seleccionar(servicio, trabajo, corregido.DocumentoVersionId, otro.DocumentoVersionId);
            Assert.DoesNotContain(actual.ArchivosPresentados, x => x.DocumentoVersionId == uno.DocumentoVersionId);
            var lista = await servicio.ListarAsync(Actor(e.Personas[1]), e.Pre.PreregistroId, new(), default);
            var original = Assert.Single(Assert.Single(lista.Items, x => x.DocumentoId == uno.DocumentoId).Versiones, x => x.Archivo.NumeroVersion == 1);
            Assert.Single(original.Evaluaciones); Assert.Single(original.Presentaciones);
            await using var verificacion = postgres.CrearContexto();
            Assert.Equal(long.Parse(e.RequisitoId), (await verificacion.Documentos.SingleAsync(x => x.DocumentoId == long.Parse(uno.DocumentoId))).PreregistroRequisitoId);
            var evaluadas = await verificacion.RevisionesDetalleDocumentos.Where(x => x.RevisionDetalle.PreregistroRequisitoId == long.Parse(e.RequisitoId)).ToListAsync();
            Assert.Equal(2, evaluadas.Count); Assert.DoesNotContain(evaluadas, x => x.DocumentoVersionId == long.Parse(corregido.DocumentoVersionId));
            var descarga = await servicio.DescargarAsync(Actor(e.Personas[1]), e.Pre.PreregistroId, uno.DocumentoId, uno.DocumentoVersionId, default);
            await using var stream = descarga.Contenido; using var copia = new MemoryStream(); await stream.CopyToAsync(copia); Assert.Equal(Pdf(), copia.ToArray());
        }

        [Fact]
        public async Task RetirarEsIdempotenteConservaArchivoYNoRequiereMatrizParaRetenerOQuitar()
        {
            var e = await CrearEscenario(); DocumentoVersionDto uno;
            using (var servicios = Servicios()) using (var scope = servicios.CreateScope())
                uno = await Cargar(scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>(), e);
            using var sinMatriz = Servicios(correspondencia: false); using var scopeSinMatriz = sinMatriz.CreateScope();
            var servicio = scopeSinMatriz.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>();
            await Seleccionar(servicio, e, uno.DocumentoVersionId);
            var retirado = await Seleccionar(servicio, e); Assert.Empty(retirado.ArchivosPresentados);
            await using var db = postgres.CrearContexto();
            var auditorias = await db.RegistrosAuditoria.CountAsync(x => x.ExpedienteId == long.Parse(e.Pre.ExpedienteId));
            await Seleccionar(servicio, e);
            Assert.Equal(auditorias, await db.RegistrosAuditoria.CountAsync(x => x.ExpedienteId == long.Parse(e.Pre.ExpedienteId)));
            Assert.Equal("PENDIENTE", retirado.EstadoCodigo);
            Assert.True(await db.DocumentosVersiones.AnyAsync(x => x.DocumentoVersionId == long.Parse(uno.DocumentoVersionId)));
            var descarga = await servicio.DescargarAsync(Actor(e.Personas[0]), e.Pre.PreregistroId, uno.DocumentoId, uno.DocumentoVersionId, default);
            await descarga.Contenido.DisposeAsync();
            await Assert.ThrowsAsync<ServicioNoDisponibleNegocioException>(() => Seleccionar(servicio, e, uno.DocumentoVersionId));
        }

        [Fact]
        public async Task CorreccionesConcurrentesConMismaBaseSoloPermitenUnaNuevaVersion()
        {
            var e = await CrearEscenario(); using var servicios = Servicios(); DocumentoVersionDto uno;
            using (var scope = servicios.CreateScope()) uno = await Cargar(scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>(), e);
            async Task<bool> Corregir()
            {
                using var scope = servicios.CreateScope();
                try
                {
                    await scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>().CorregirAsync(Actor(e.Personas[0]),
                        e.Pre.PreregistroId, e.Version.PreregistroVersionId, e.RequisitoId, uno.DocumentoId, uno.DocumentoVersionId,
                        "CORRECCIÓN CONCURRENTE FICTICIA", Entrada(Pdf("nueva")), default); return true;
                }
                catch (ConflictoNegocioException) { return false; }
            }
            var resultados = await Task.WhenAll(Corregir(), Corregir()); Assert.Single(resultados, x => x); Assert.Single(resultados, x => !x);
            await using var db = postgres.CrearContexto();
            Assert.Equal(new[] { 1, 2 }, await db.DocumentosVersiones.Where(x => x.DocumentoId == long.Parse(uno.DocumentoId)).OrderBy(x => x.NumeroVersion).Select(x => x.NumeroVersion).ToArrayAsync());
            Assert.Single(await db.PreregistrosRequisitosDocumentos.Where(x => x.PreregistroRequisitoId == long.Parse(e.RequisitoId)).ToListAsync());
            Assert.Equal(2, Directory.GetFiles(raiz, "*.bin", SearchOption.AllDirectories).Length);
        }

        [Fact]
        public async Task FalloAuditoriaRevierteFilasYCompensaSoloElArchivoNuevo()
        {
            var e = await CrearEscenario(); DocumentoVersionDto existente;
            using (var servicios = Servicios()) using (var scope = servicios.CreateScope()) existente = await Cargar(scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>(), e);
            using (var servicios = Servicios(new FallarAuditoria())) using (var scope = servicios.CreateScope())
                await Assert.ThrowsAsync<InvalidOperationException>(() => Cargar(scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>(), e));
            await using var db = postgres.CrearContexto();
            Assert.Single(await db.Documentos.Where(x => x.ExpedienteId == long.Parse(e.Pre.ExpedienteId)).ToListAsync());
            Assert.Single(await db.PreregistrosRequisitosDocumentos.Where(x => x.PreregistroRequisitoId == long.Parse(e.RequisitoId)).ToListAsync());
            Assert.Single(Directory.GetFiles(raiz, "*.bin", SearchOption.AllDirectories));
            using var correctos = Servicios(); using var correcto = correctos.CreateScope();
            var descarga = await correcto.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>().DescargarAsync(Actor(e.Personas[0]), e.Pre.PreregistroId, existente.DocumentoId, existente.DocumentoVersionId, default);
            await descarga.Contenido.DisposeAsync();
        }

        [Theory]
        [InlineData("ENVIADO")]
        [InlineData("APROBADO")]
        [InlineData("SUPERADA")]
        [InlineData("REVISION")]
        [InlineData("NO_APLICA")]
        public async Task EscrituraRechazaVersionCongeladaORequisitoNoAplicable(string condicion)
        {
            var e = await CrearEscenario(); using var servicios = Servicios(); using var scope = servicios.CreateScope();
            if (condicion == "SUPERADA")
                await scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>().CrearAsync(Actor(e.Personas[0]), e.Pre.PreregistroId,
                    PaqueteCPruebas.DatosVersion(e.Version.PreregistroVersionId), default);
            else
            {
                await using var db = postgres.CrearContexto();
                var pre = await db.Preregistros.SingleAsync(x => x.PreregistroId == long.Parse(e.Pre.PreregistroId));
                if (condicion == "NO_APLICA")
                {
                    var req = await db.PreregistrosRequisitos.SingleAsync(x => x.PreregistroRequisitoId == long.Parse(e.RequisitoId)); req.Aplica = false; req.EstadoCodigo = "NO_APLICA";
                }
                else if (condicion == "REVISION")
                    db.RevisionesPrerregistro.Add(new() { PreregistroVersionId = long.Parse(e.Version.PreregistroVersionId), NumeroRevision = 1,
                        RevisadoPorUsuario = new() { Login = "prueba-d-" + Guid.NewGuid().ToString("N"), NombreVisible = "ABOGADA FICTICIA", PasswordHash = "FICTICIO", RolCodigo = "ABOGADA" }, IniciadaEn = e.Version.CreadoEn });
                else
                {
                    pre.EstadoCodigo = condicion; pre.EnviadoEn = e.Version.CreadoEn;
                    if (condicion == "APROBADO") pre.AprobadoEn = e.Version.CreadoEn;
                }
                await db.SaveChangesAsync();
            }
            await Assert.ThrowsAsync<ConflictoNegocioException>(() => Cargar(scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>(), e));
            Assert.False(Directory.Exists(raiz));
        }

        [Theory]
        [InlineData(false, true, "POLITICA_ARCHIVOS_PENDIENTE")]
        [InlineData(true, false, "TIPOS_REQUISITO_PENDIENTES")]
        public async Task ConfiguracionPendienteNoCreaArchivosNiInventaCorrespondencias(bool politica, bool correspondencia, string codigo)
        {
            var e = await CrearEscenario(); using var servicios = Servicios(politica: politica, correspondencia: correspondencia); using var scope = servicios.CreateScope();
            var error = await Assert.ThrowsAsync<ServicioNoDisponibleNegocioException>(() => Cargar(scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>(), e));
            Assert.Equal(codigo, error.Codigo); Assert.False(Directory.Exists(raiz));
            await using var db = postgres.CrearContexto(); Assert.False(await db.Documentos.AnyAsync(x => x.ExpedienteId == long.Parse(e.Pre.ExpedienteId)));
        }

        [Theory]
        [InlineData("VACIO", 400)]
        [InlineData("EXTENSION", 415)]
        [InlineData("CONTENIDO", 415)]
        [InlineData("PDF_SIMULADO", 415)]
        [InlineData("PDF_TRUNCADO", 415)]
        [InlineData("PDF_SIN_PAGINAS", 415)]
        [InlineData("PDF_CIFRADO", 415)]
        [InlineData("JPEG", 415)]
        [InlineData("DECLARADO_GRANDE", 413)]
        [InlineData("REAL_GRANDE", 413)]
        public async Task CargasInvalidasNoDejanFilasNiBytes(string caso, int estado)
        {
            var e = await CrearEscenario(); using var servicios = Servicios(); using var scope = servicios.CreateScope();
            var entrada = caso switch
            {
                "VACIO" => Entrada([]), "EXTENSION" => Entrada(nombre: "archivo.exe"),
                "CONTENIDO" => Entrada(Encoding.ASCII.GetBytes("No es un PDF")),
                "PDF_SIMULADO" => Entrada(Encoding.ASCII.GetBytes("%PDF-1.7\nTexto sin estructura\n%%EOF\n")),
                "PDF_TRUNCADO" => Entrada(Pdf()[..^20]),
                "PDF_SIN_PAGINAS" => Entrada(Pdf(sinPaginas: true)),
                // PDF real cifrado, generado con pypdf para prueba; incluso la contraseña vacía no debe habilitarlo.
                "PDF_CIFRADO" => Entrada(Convert.FromBase64String("JVBERi0xLjMKJeLjz9MKMSAwIG9iago8PAovUHJvZHVjZXIgPDU0ZGMwYWQ1ZDE+Cj4+CmVuZG9iagoyIDAgb2JqCjw8Ci9UeXBlIC9QYWdlcwovQ291bnQgMQovS2lkcyBbIDQgMCBSIF0KPj4KZW5kb2JqCjMgMCBvYmoKPDwKL1R5cGUgL0NhdGFsb2cKL1BhZ2VzIDIgMCBSCj4+CmVuZG9iago0IDAgb2JqCjw8Ci9UeXBlIC9QYWdlCi9SZXNvdXJjZXMgPDwKPj4KL01lZGlhQm94IFsgMC4wIDAuMCA2MTIgNzkyIF0KL1BhcmVudCAyIDAgUgo+PgplbmRvYmoKNSAwIG9iago8PAovViAyCi9SIDMKL0xlbmd0aCAxMjgKL1AgNDI5NDk2NzI5MgovRmlsdGVyIC9TdGFuZGFyZAovTyA8YjQ4ODQ2OGIwNWQ1ZTZmZGVlYmE1ODEzOWNlNmFhZDNkNDExZGVjNjE2Nzg3MDgxM2ZiOTgyZmQ5NWZiOWIzMj4KL1UgPGFjMWM5NjhlYmYyZmVmODA1YzNmODNlMjFlMGYxZDQxMjhiZjRlNWU0ZTc1OGE0MTY0MDA0ZTU2ZmZmYTAxMDg+Cj4+CmVuZG9iagp4cmVmCjAgNgowMDAwMDAwMDAwIDY1NTM1IGYgCjAwMDAwMDAwMTUgMDAwMDAgbiAKMDAwMDAwMDA1OSAwMDAwMCBuIAowMDAwMDAwMTE4IDAwMDAwIG4gCjAwMDAwMDAxNjcgMDAwMDAgbiAKMDAwMDAwMDI2MSAwMDAwMCBuIAp0cmFpbGVyCjw8Ci9TaXplIDYKL1Jvb3QgMyAwIFIKL0luZm8gMSAwIFIKL0lEIFsgPDM1NjEzMTMyNjIzNzY0MzczODM1NjEzNjY0MzUzNTM3MzUzNjM5NjI2MjM3MzA2NDMyMzQzMjMyNjEzNzMwMzk+IDwzNTYxMzEzMjYyMzc2NDM3MzgzNTYxMzY2NDM1MzUzNzM1MzYzOTYyNjIzNzMwNjQzMjM0MzIzMjYxMzczMDM5PiBdCi9FbmNyeXB0IDUgMCBSCj4+CnN0YXJ0eHJlZgo0NzYKJSVFT0YK")),
                "JPEG" => Entrada([0xff, 0xd8, 0xff, 0xff, 0xd9], "imagen.jpg"),
                "DECLARADO_GRANDE" => Entrada(declarado: 2048),
                _ => Entrada(Pdf(new string('x', 2048)), declarado: 10)
            };
            var error = await Assert.ThrowsAsync<ArchivoNegocioException>(() => Cargar(scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>(), e, entrada));
            Assert.Equal(estado, error.EstadoHttp);
            Assert.Empty(Directory.Exists(raiz) ? Directory.GetFiles(raiz, "*", SearchOption.AllDirectories) : []);
            await using var db = postgres.CrearContexto(); Assert.False(await db.Documentos.AnyAsync(x => x.ExpedienteId == long.Parse(e.Pre.ExpedienteId)));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task DescargaRechazaBytesAusentesOAlteradosSinTocarHistorial(bool ausente)
        {
            var e = await CrearEscenario(); using var servicios = Servicios(); using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>(); var uno = await Cargar(servicio, e);
            await using var db = postgres.CrearContexto();
            var archivo = await db.DocumentosVersiones.SingleAsync(x => x.DocumentoVersionId == long.Parse(uno.DocumentoVersionId));
            var ruta = Path.Combine(raiz, archivo.AlmacenamientoClave.Replace('/', Path.DirectorySeparatorChar));
            if (ausente) File.Delete(ruta); else await File.WriteAllBytesAsync(ruta, Pdf("alteracion"));
            var error = await Assert.ThrowsAsync<ServicioNoDisponibleNegocioException>(() => servicio.DescargarAsync(Actor(e.Personas[0]), e.Pre.PreregistroId, uno.DocumentoId, uno.DocumentoVersionId, default));
            Assert.Equal("ARCHIVO_NO_DISPONIBLE", error.Codigo);
            Assert.Equal(uno.Sha256, (await db.DocumentosVersiones.AsNoTracking().SingleAsync(x => x.DocumentoVersionId == archivo.DocumentoVersionId)).Sha256);
            Assert.True(await db.PreregistrosRequisitosDocumentos.AnyAsync(x => x.DocumentoVersionId == archivo.DocumentoVersionId));
        }

        [Fact]
        public async Task SeleccionRechazaIdsRepetidosDosVersionesMismoDocumentoYArchivosDeOtroExpediente()
        {
            var e = await CrearEscenario(); var otro = await CrearEscenario(); using var servicios = Servicios(); using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>(); var uno = await Cargar(servicio, e); var ajeno = await Cargar(servicio, otro);
            var dos = await servicio.CorregirAsync(Actor(e.Personas[0]), e.Pre.PreregistroId, e.Version.PreregistroVersionId, e.RequisitoId,
                uno.DocumentoId, uno.DocumentoVersionId, "CORRECCIÓN FICTICIA", Entrada(Pdf("nuevo")), default);
            await Assert.ThrowsAsync<ValidationException>(() => Seleccionar(servicio, e, dos.DocumentoVersionId, dos.DocumentoVersionId));
            await Assert.ThrowsAsync<ValidationException>(() => Seleccionar(servicio, e, uno.DocumentoVersionId, dos.DocumentoVersionId));
            await Assert.ThrowsAsync<RecursoNoEncontradoNegocioException>(() => Seleccionar(servicio, e, ajeno.DocumentoVersionId));
            await Assert.ThrowsAsync<RecursoNoEncontradoNegocioException>(() => servicio.DescargarAsync(Actor(e.Personas[0]), otro.Pre.PreregistroId, uno.DocumentoId, uno.DocumentoVersionId, default));
        }

        [Fact]
        public async Task AlmacenamientoImpideEscapeDeRutaYCancelaSinArchivoFinal()
        {
            var almacen = new AlmacenamientoArchivosLocal(Options.Create(new AlmacenamientoArchivosOpciones { DirectorioRaiz = raiz }));
            await Assert.ThrowsAsync<ArchivoPersistenciaException>(() => almacen.AbrirVerificadoAsync("../../otro.pdf", new string('a', 64), 1, "application/pdf", default));
            using var cancelado = new CancellationTokenSource(); cancelado.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => almacen.GuardarNuevoAsync(new MemoryStream(Pdf()), 1024, "application/pdf", cancelado.Token));
            Assert.Empty(Directory.Exists(raiz) ? Directory.GetFiles(raiz, "*", SearchOption.AllDirectories) : []);
        }

        [Fact]
        public async Task FalloAuditoriaDuranteCorreccionConservaSeleccionYBytesOriginales()
        {
            var e = await CrearEscenario(); DocumentoVersionDto uno;
            using (var servicios = Servicios()) using (var scope = servicios.CreateScope()) uno = await Cargar(scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>(), e);
            using (var servicios = Servicios(new FallarAuditoria())) using (var scope = servicios.CreateScope())
                await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>().CorregirAsync(
                    Actor(e.Personas[0]), e.Pre.PreregistroId, e.Version.PreregistroVersionId, e.RequisitoId,
                    uno.DocumentoId, uno.DocumentoVersionId, "CORRECCIÓN FALLIDA FICTICIA", Entrada(Pdf("cambio")), default));
            await using var db = postgres.CrearContexto();
            Assert.Single(await db.DocumentosVersiones.Where(x => x.DocumentoId == long.Parse(uno.DocumentoId)).ToListAsync());
            Assert.Equal(long.Parse(uno.DocumentoVersionId), Assert.Single(await db.PreregistrosRequisitosDocumentos.Where(x => x.PreregistroRequisitoId == long.Parse(e.RequisitoId)).ToListAsync()).DocumentoVersionId);
            Assert.Single(Directory.GetFiles(raiz, "*.bin", SearchOption.AllDirectories));
        }

        [Theory]
        [InlineData("INACTIVO")]
        [InlineData("MUNICIPALIDAD")]
        [InlineData("OTRO_TIPO")]
        public async Task NuevasCargasRespetanTipoConfirmadoYOrigenCiudadano(string caso)
        {
            var e = await CrearEscenario();
            await using (var db = postgres.CrearContexto())
            {
                var tipo = await db.TiposDocumento.SingleAsync(x => x.TipoDocumentoId == e.TipoId);
                if (caso == "INACTIVO") tipo.Activo = false;
                else if (caso == "MUNICIPALIDAD") tipo.OrigenCodigo = "MUNICIPALIDAD";
                else
                {
                    tipo = new TipoDocumento { Codigo = "FICTICIO-OTRO-" + Guid.NewGuid().ToString("N")[..8], Nombre = "TIPO FICTICIO AJENO", OrigenCodigo = "CIUDADANO" };
                    db.TiposDocumento.Add(tipo);
                }
                await db.SaveChangesAsync(); e = e with { TipoId = tipo.TipoDocumentoId };
            }
            using var servicios = Servicios(); using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>();
            if (caso == "OTRO_TIPO") await Assert.ThrowsAsync<ValidationException>(() => Cargar(servicio, e));
            else await Assert.ThrowsAsync<ConflictoNegocioException>(() => Cargar(servicio, e));
            Assert.False(Directory.Exists(raiz));
        }

        [Fact]
        public async Task LecturaNoExponeAsociacionesHistoricasIncoherentesConOtroPre()
        {
            var e = await CrearEscenario(); var otro = await CrearEscenario(); using var servicios = Servicios(); using var scope = servicios.CreateScope();
            var servicio = scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>(); var uno = await Cargar(servicio, e);
            await using var db = postgres.CrearContexto();
            // Las FK individuales permiten esta corrupción fuera del servicio; la API debe detectarla.
            db.PreregistrosRequisitosDocumentos.Add(new() { PreregistroRequisitoId = long.Parse(otro.RequisitoId), DocumentoVersionId = long.Parse(uno.DocumentoVersionId) });
            await db.SaveChangesAsync();
            await Assert.ThrowsAsync<ConflictoNegocioException>(() => servicio.ListarAsync(Actor(e.Personas[0]), e.Pre.PreregistroId, new(), default));
            await Assert.ThrowsAsync<ConflictoNegocioException>(() => Seleccionar(servicio, otro));
        }

        [Fact]
        public async Task ApiPoliticaPendienteRechazaCargaAntesDeCrearBytesOFilas()
        {
            var e = await CrearEscenario(); using var fabrica = new ApiAislada(postgres, x => Configurar(x, politica: false));
            using var cliente = fabrica.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
            var login = await cliente.PostAsJsonAsync("/api/acceso-ciudadano/sesion", new { dni = e.Personas[0].Dni, sufijoDireccion = "123" });
            cliente.DefaultRequestHeaders.Authorization = new("Bearer", (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString());
            using var form = new MultipartFormDataContent(); form.Add(new ByteArrayContent(Pdf()), "archivo", "archivo.pdf");
            form.Add(new StringContent(e.TipoId.ToString()), "tipoDocumentoId"); form.Add(new StringContent("FICTICIO"), "titulo");
            var respuesta = await cliente.PostAsync($"/api/preregistros/{e.Pre.PreregistroId}/versiones/{e.Version.PreregistroVersionId}/requisitos/{e.RequisitoId}/documentos", form);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, respuesta.StatusCode);
            Assert.Equal("POLITICA_ARCHIVOS_PENDIENTE", (await respuesta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("codigo").GetString());
            Assert.False(Directory.Exists(raiz)); Assert.Equal(0, fabrica.Proveedor.Llamadas);
        }

        [Fact]
        public async Task ApiMultipartDescargaPrivadaErroresYSwaggerDeLosCincoEndpoints()
        {
            var e = await CrearEscenario(); using var fabrica = new ApiAislada(postgres, x => Configurar(x));
            using var cliente = fabrica.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
            var prefijo = "/api/preregistros/" + e.Pre.PreregistroId;
            var ruta = prefijo + "/versiones/" + e.Version.PreregistroVersionId + "/requisitos/" + e.RequisitoId + "/documentos";
            Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync(prefijo + "/documentos")).StatusCode);
            async Task Login(int indice)
            {
                var sesion = await cliente.PostAsJsonAsync("/api/acceso-ciudadano/sesion", new { dni = e.Personas[indice].Dni, sufijoDireccion = "123" });
                Assert.Equal(HttpStatusCode.OK, sesion.StatusCode);
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (await sesion.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString());
            }
            MultipartFormDataContent Formulario(byte[]? bytes = null, string nombre = "archivo.pdf", bool extra = false)
            {
                var formulario = new MultipartFormDataContent(); var archivo = new ByteArrayContent(bytes ?? Pdf());
                archivo.Headers.ContentType = new("application/octet-stream"); // No confiar en el MIME enviado.
                formulario.Add(archivo, "archivo", nombre); formulario.Add(new StringContent(e.TipoId.ToString()), "tipoDocumentoId");
                formulario.Add(new StringContent("EVIDENCIA FICTICIA"), "titulo");
                if (extra) formulario.Add(new StringContent("999"), "creadoPorCuentaId");
                return formulario;
            }
            await Login(0);
            using (var form = Formulario(extra: true)) Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PostAsync(ruta, form)).StatusCode);
            using (var form = Formulario(nombre: "archivo.exe")) Assert.Equal(HttpStatusCode.UnsupportedMediaType, (await cliente.PostAsync(ruta, form)).StatusCode);
            using (var form = Formulario(Pdf(new string('x', 2048)))) Assert.Equal(HttpStatusCode.RequestEntityTooLarge, (await cliente.PostAsync(ruta, form)).StatusCode);
            DocumentoVersionDto uno;
            using (var form = Formulario())
            {
                var respuesta = await cliente.PostAsync(ruta, form); Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
                uno = (await respuesta.Content.ReadFromJsonAsync<DocumentoVersionDto>())!; Assert.Equal(uno.UrlDescarga, respuesta.Headers.Location!.ToString());
            }
            using (var form = new MultipartFormDataContent())
            {
                form.Add(new ByteArrayContent(Pdf("corregido")), "archivo", "correccion.pdf");
                form.Add(new StringContent("CORRECCIÓN FICTICIA"), "motivoCambio"); form.Add(new StringContent(uno.DocumentoVersionId), "documentoVersionBaseId");
                Assert.Equal(HttpStatusCode.Created, (await cliente.PostAsync(ruta + "/" + uno.DocumentoId + "/versiones", form)).StatusCode);
            }
            var selection = ruta[..^"documentos".Length] + "archivos-presentados";
            Assert.Equal(HttpStatusCode.OK, (await cliente.PutAsJsonAsync(selection, new { documentoVersionIds = new[] { uno.DocumentoVersionId } })).StatusCode);
            var descarga = await cliente.GetAsync(uno.UrlDescarga); Assert.Equal(HttpStatusCode.OK, descarga.StatusCode);
            Assert.Equal(Pdf(), await descarga.Content.ReadAsByteArrayAsync()); Assert.Equal("no-store", descarga.Headers.CacheControl!.ToString());
            Assert.Equal("nosniff", Assert.Single(descarga.Headers.GetValues("X-Content-Type-Options")));
            Assert.Equal("attachment", descarga.Content.Headers.ContentDisposition!.DispositionType);
            await Login(1); Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync(uno.UrlDescarga)).StatusCode);
            using (var form = Formulario()) Assert.Equal(HttpStatusCode.Forbidden, (await cliente.PostAsync(ruta, form)).StatusCode);
            await Login(2); Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync(uno.UrlDescarga)).StatusCode);
            var swagger = await cliente.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
            var paths = swagger.GetProperty("paths");
            foreach (var (path, method) in new[] {
                ("/api/preregistros/{preregistroId}/versiones/{versionId}/requisitos/{requisitoId}/documentos", "post"),
                ("/api/preregistros/{preregistroId}/versiones/{versionId}/requisitos/{requisitoId}/documentos/{documentoId}/versiones", "post"),
                ("/api/preregistros/{preregistroId}/versiones/{versionId}/requisitos/{requisitoId}/archivos-presentados", "put"),
                ("/api/preregistros/{preregistroId}/documentos", "get"),
                ("/api/preregistros/{preregistroId}/documentos/{documentoId}/versiones/{documentoVersionId}/archivo", "get") })
                Assert.True(paths.GetProperty(path).GetProperty(method).TryGetProperty("security", out _));
            Assert.Equal(0, fabrica.Proveedor.Llamadas);
        }

        public void Dispose()
        {
            var directorio = Path.GetFullPath(raiz);
            var esperado = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!directorio.StartsWith(esperado, StringComparison.OrdinalIgnoreCase)
                || !Path.GetFileName(directorio).StartsWith("divorcios-archivos-prueba-", StringComparison.Ordinal))
                throw new InvalidOperationException("Sólo se limpia el directorio temporal de esta prueba.");
            if (Directory.Exists(directorio)) Directory.Delete(directorio, recursive: true);
        }
    }
}
