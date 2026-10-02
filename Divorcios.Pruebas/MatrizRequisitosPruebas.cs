using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Divorcios.Datos.Contexto;
using Divorcios.Datos.Interfaces;
using Divorcios.Datos.Opciones;
using Divorcios.Dominio.Entidades;
using Divorcios.Negocio.DTOs.Preregistro;
using Divorcios.Negocio.Extensiones;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Matrices;
using Divorcios.Negocio.Opciones;
using Divorcios.Negocio.Resultados;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using static Divorcios.Pruebas.PaqueteBPruebas;

namespace Divorcios.Pruebas;

[Collection("PostgreSQL aislado")]
public sealed class MatrizRequisitosPruebas(PostgresAislado postgres) : IDisposable
{
    private readonly PaqueteBPruebas apoyo = new(postgres);
    private readonly string raiz = Path.Combine(Path.GetTempPath(), "divorcios-matriz-prueba-" + Guid.NewGuid().ToString("N"));
    private static PreregistroVersion Encuesta() => new() { CantidadHijosMayores = 2, RequiereRepresentacionA = false,
        RequiereRepresentacionB = false, MatrimonioEnPorvenir = false, UltimoDomicilioConyugalPorvenir = true };
    private static DefinicionRequisito[] Definiciones()
    {
        var menores = Encuesta(); menores.CantidadHijosMenores = 2;
        menores.TieneHijosMayoresSituacionEspecial = true; menores.RequiereRepresentacionA = true; menores.RequiereRepresentacionB = true;
        var negativas = Encuesta(); negativas.TieneHijosMayoresSituacionEspecial = false;
        return MatrizRequisitosPreregistro.Determinar(menores).Requisitos.Concat(MatrizRequisitosPreregistro.Determinar(negativas).Requisitos)
            .DistinctBy(x => x.Codigo).ToArray();
    }
    private void Configurar(IServiceCollection servicios, bool incompleta = false)
    {
        servicios.Configure<MatrizRequisitosOpciones>(x => x.HabilitarGeneracionParcial = true);
        servicios.Configure<InformacionPreregistroOpciones>(x => { x.MaximoBytes = 10000000; x.MimePermitidos = ["application/pdf"]; });
        servicios.Configure<AlmacenamientoArchivosOpciones>(x => x.DirectorioRaiz = raiz);
        servicios.Configure<ArchivosPreregistroOpciones>(x => x.TiposPorRequisito = Definiciones().Select(d => new TiposRequisitoOpciones
        { RequisitoCodigo = d.Codigo, Confirmada = true, TiposDocumentoCodigos = incompleta ? [] : d.TiposDocumentoCodigos.ToList() }).ToList());
    }
    private ServiceProvider Servicios(IInterceptor? fallo = null, bool incompleta = false, Action<IServiceCollection>? extra = null)
    {
        var servicios = new ServiceCollection(); servicios.AddLogging(); servicios.AgregarCapaNegocio(postgres.Conexion);
        servicios.RemoveAll<DivorciosDbContext>(); servicios.AddScoped(_ => postgres.CrearContexto(fallo));
        servicios.AddSingleton<IReniecProveedor>(new ProveedorProhibido()); Configurar(servicios, incompleta); extra?.Invoke(servicios);
        return servicios.BuildServiceProvider();
    }
    private async Task Catalogos()
    {
        // Sólo PostgreSQL aleatorio del fixture. Nunca la conexión de desarrollo ni sus volúmenes.
        await using var db = postgres.CrearContexto();
        foreach (var d in Definiciones())
            if (!await db.RequisitosCatalogo.AnyAsync(x => x.Codigo == d.Codigo))
                db.RequisitosCatalogo.Add(new() { Codigo = d.Codigo, Nombre = d.Nombre });
        foreach (var t in Definiciones().SelectMany(x => x.TiposDocumentoCodigos).Distinct())
            if (!await db.TiposDocumento.AnyAsync(x => x.Codigo == t))
                db.TiposDocumento.Add(new() { Codigo = t, Nombre = t, OrigenCodigo = "CIUDADANO" });
        await db.SaveChangesAsync();
    }
    private static CrearVersionPreregistroDto Datos(string? anterior = null)
    {
        var datos = PaqueteCPruebas.DatosVersion(anterior);
        datos.Encuesta.MatrimonioEnPorvenir = false; datos.Encuesta.UltimoDomicilioConyugalPorvenir = true;
        datos.Encuesta.DomicilioConyugal = "DOMICILIO FICTICIO";
        datos.Encuesta.TieneHijosMayoresSituacionEspecial = false;
        datos.Encuesta.RequiereRepresentacionA = true; datos.Encuesta.RequiereRepresentacionB = false;
        return datos;
    }
    private async Task<(PersonaPrueba[] Personas, PreregistroDetalleDto Pre)> Escenario()
    {
        var personas = await apoyo.Personas(cuentaSegundo: true);
        using var servicios = Servicios(); using var scope = servicios.CreateScope();
        return (personas, await scope.ServiceProvider.GetRequiredService<IPreregistrosServicio>().CrearAsync(Actor(personas[0]), PaqueteBPruebas.Datos(personas), default));
    }

    [Theory]
    [InlineData(null, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, true)]
    public void MayoresOrdinariosNullYEspecialTienenRamasDistintas(bool? especial, bool negativa, bool nacimiento)
    {
        var encuesta = Encuesta(); encuesta.TieneHijosMayoresSituacionEspecial = especial;
        var resultado = MatrizRequisitosPreregistro.Determinar(encuesta);
        Assert.Equal(negativa, resultado.Requisitos.Any(x => x.Codigo == "PRE_DJ_SIN_MAYORES_ESPECIAL"));
        Assert.Equal(nacimiento, resultado.Requisitos.Any(x => x.Codigo == "PRE_NACIMIENTOS_MAYORES_ESPECIAL"));
        Assert.DoesNotContain(resultado.Requisitos, x => x.TiposDocumentoCodigos.Any(t => t.Contains("CURATELA") || t.Contains("INTERDICCION")));
        if (especial is null) Assert.Contains("RESPUESTA_PENDIENTE_SITUACION_ESPECIAL_MAYORES", resultado.Pendientes);
        if (especial == true) Assert.Contains("MAYORES_SUPUESTO_Y_DOCUMENTACION_REVISION_JURIDICA", resultado.Pendientes);
    }
    [Theory]
    [InlineData(null, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void RepresentacionSeEvaluaPorConyuge(bool? a, bool? b)
    {
        var encuesta = Encuesta(); encuesta.RequiereRepresentacionA = a; encuesta.RequiereRepresentacionB = b;
        var resultado = MatrizRequisitosPreregistro.Determinar(encuesta);
        Assert.Equal(a == true, resultado.Requisitos.Any(x => x.Codigo == "PRE_PODER_A"));
        Assert.Equal(b == true, resultado.Requisitos.Any(x => x.Codigo == "PRE_PODER_B"));
        Assert.Equal(a is null, resultado.Pendientes.Contains("RESPUESTA_PENDIENTE_REPRESENTACION_A"));
    }
    [Fact]
    public void MenoresCubrenCadaHijoConAlternativasSinFijarCantidadDePdf()
    {
        var e = Encuesta(); e.CantidadHijosMenores = 3;
        var m = MatrizRequisitosPreregistro.Determinar(e);
        var nacimiento = Assert.Single(m.Requisitos, x => x.Codigo == "PRE_NACIMIENTOS_MENORES");
        Assert.Equal(3, nacimiento.CantidadTitulares); Assert.Equal("CADA_MENOR", nacimiento.TitularesCodigo);
        Assert.Equal(2, Assert.Single(m.Requisitos, x => x.Codigo == "PRE_REGIMEN_MENORES").TiposDocumentoCodigos.Count);
        Assert.DoesNotContain(m.Requisitos, x => x.Codigo == "PRE_DJ_SIN_MENORES");
    }
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void CompetenciaSeSeparaDeEvidencia(bool matrimonio, bool domicilio)
    {
        var e = Encuesta(); e.MatrimonioEnPorvenir = matrimonio; e.UltimoDomicilioConyugalPorvenir = domicilio;
        var m = MatrizRequisitosPreregistro.Determinar(e);
        Assert.Equal(!matrimonio && domicilio, m.Requisitos.Any(x => x.Codigo == "PRE_DOMICILIO_CONYUGAL"));
        Assert.Contains(m.Requisitos, x => x.Codigo == "PRE_MATRIMONIO");
        if (!matrimonio && !domicilio) Assert.Contains("COMPETENCIA_TERRITORIAL_PENDIENTE_REVISION", m.Pendientes);
    }
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void BienesGenericosNoGeneranRequisitosPatrimoniales(bool bienes, bool acuerdo)
    {
        var e = Encuesta(); e.TieneBienes = bienes; e.TieneAcuerdoBienes = acuerdo;
        var m = MatrizRequisitosPreregistro.Determinar(e);
        Assert.Contains("PATRIMONIO_CLASIFICACION_PENDIENTE", m.Pendientes);
        Assert.DoesNotContain(m.Requisitos, x => x.Codigo.Contains("PATRIMON") || x.Codigo.Contains("GANANCIALES"));
    }
    [Fact]
    public async Task GeneracionYFuentesQuedanConLaEncuestaSinCompletarElPreregistro()
    {
        await Catalogos(); var (personas, pre) = await Escenario();
        using var servicios = Servicios(); using var scope = servicios.CreateScope();
        var v = scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>();
        var creada = await v.CrearAsync(Actor(personas[0]), pre.PreregistroId, Datos(), default);
        Assert.Equal("GENERACION_PARCIAL", creada.EstadoGeneracionRequisitosCodigo);
        var r = await v.ObtenerRequisitosAsync(Actor(personas[1]), pre.PreregistroId, creada.PreregistroVersionId, default);
        Assert.Equal(6, r.Items.Count); Assert.NotEmpty(r.PendientesConfiguracion);
        Assert.All(r.Items, x => { Assert.Equal("PENDIENTE", x.EstadoCodigo); Assert.Empty(x.ArchivosPresentados);
            Assert.Equal(MatrizRequisitosPreregistro.Version, x.Determinacion!.MatrizVersion); Assert.NotEmpty(x.Determinacion.Fuentes); });
        var pendiente = Assert.Single(r.Items, x => x.Codigo == "PRE_PODER_A"); Assert.Equal("A", pendiente.Determinacion!.TitularesCodigo);
        Assert.DoesNotContain(r.Items, x => x.Codigo == "PRE_PODER_B");
        using var sinReglas = Servicios(incompleta: true); using var otraScope = sinReglas.CreateScope();
        var antigua = await otraScope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>()
            .ObtenerRequisitosAsync(Actor(personas[0]), pre.PreregistroId, creada.PreregistroVersionId, default);
        Assert.Equal(r.Items.Select(x => x.Determinacion!.NombreAlGenerar), antigua.Items.Select(x => x.Determinacion!.NombreAlGenerar));
        Assert.Equal(r.PendientesConfiguracion, antigua.PendientesConfiguracion);
    }
    [Fact]
    public async Task ConfiguracionIncompletaNoCreaRequisitosNiRellenaHistoria()
    {
        await Catalogos(); var (personas, pre) = await Escenario();
        using var servicios = Servicios(incompleta: true); using var scope = servicios.CreateScope();
        var s = scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>();
        var creada = await s.CrearAsync(Actor(personas[0]), pre.PreregistroId, Datos(), default);
        var r = await s.ObtenerRequisitosAsync(Actor(personas[0]), pre.PreregistroId, creada.PreregistroVersionId, default);
        Assert.Empty(r.Items); Assert.Equal("PENDIENTE_CONFIGURACION", r.EstadoGeneracionCodigo);
        Assert.Contains("CONFIGURACION_REQUISITO_PRE_SOLICITUD", r.PendientesConfiguracion);
        using var completas = Servicios(); using var otra = completas.CreateScope();
        var v = otra.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>();
        Assert.Empty((await v.ObtenerRequisitosAsync(Actor(personas[0]), pre.PreregistroId, creada.PreregistroVersionId, default)).Items);
        Assert.Equal("GENERACION_PARCIAL", (await v.CrearAsync(Actor(personas[0]), pre.PreregistroId, Datos(creada.PreregistroVersionId), default)).EstadoGeneracionRequisitosCodigo);
    }
    [Fact]
    public async Task FalloDeAuditoriaRevierteEncuestaContactosYRequisitos()
    {
        await Catalogos(); var (personas, pre) = await Escenario();
        using var servicios = Servicios(new FallarAuditoria()); using var scope = servicios.CreateScope();
        await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>()
            .CrearAsync(Actor(personas[0]), pre.PreregistroId, Datos(), default));
        await using var db = postgres.CrearContexto(); var p = long.Parse(pre.PreregistroId);
        Assert.False(await db.PreregistrosVersiones.AnyAsync(x => x.PreregistroId == p));
        Assert.False(await db.PreregistrosRequisitos.AnyAsync(x => x.PreregistroVersion.PreregistroId == p));
        Assert.False(await db.ExpedientesContactosHistorial.AnyAsync(x => x.ExpedienteConyuge.ExpedienteId == long.Parse(pre.ExpedienteId)));
    }
    [Fact]
    public async Task ReutilizacionYCorreccionDeRequisitoGeneradoConservanVersionAnterior()
    {
        await Catalogos(); var (personas, pre) = await Escenario();
        using var servicios = Servicios(); using var scope = servicios.CreateScope();
        var v = scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>(); var a = scope.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>();
        var uno = await v.CrearAsync(Actor(personas[0]), pre.PreregistroId, Datos(), default);
        var r1 = Assert.Single((await v.ObtenerRequisitosAsync(Actor(personas[0]), pre.PreregistroId, uno.PreregistroVersionId, default)).Items, x => x.Codigo == "PRE_SOLICITUD");
        await using var db = postgres.CrearContexto(); var tipo = await db.TiposDocumento.SingleAsync(x => x.Codigo == "SOL_SEPARACION_CONVENCIONAL");
        static ArchivoEntrada Archivo(string contenido) => new(new MemoryStream(PaqueteDPruebas.Pdf(contenido)), "solicitud.pdf", PaqueteDPruebas.Pdf(contenido).Length);
        var original = await a.CargarAsync(Actor(personas[0]), pre.PreregistroId, uno.PreregistroVersionId, r1.PreregistroRequisitoId, tipo.TipoDocumentoId, "Solicitud ficticia", Archivo("original"), default);
        var dos = await v.CrearAsync(Actor(personas[0]), pre.PreregistroId, Datos(uno.PreregistroVersionId), default);
        var r2 = Assert.Single((await v.ObtenerRequisitosAsync(Actor(personas[0]), pre.PreregistroId, dos.PreregistroVersionId, default)).Items, x => x.Codigo == "PRE_SOLICITUD");
        await a.SeleccionarAsync(Actor(personas[0]), pre.PreregistroId, dos.PreregistroVersionId, r2.PreregistroRequisitoId, new() { DocumentoVersionIds = [original.DocumentoVersionId] }, default);
        var corregida = await a.CorregirAsync(Actor(personas[0]), pre.PreregistroId, dos.PreregistroVersionId, r2.PreregistroRequisitoId, original.DocumentoId, original.DocumentoVersionId, "Corrección ficticia", Archivo("correccion"), default);
        var vieja = Assert.Single((await v.ObtenerRequisitosAsync(Actor(personas[1]), pre.PreregistroId, uno.PreregistroVersionId, default)).Items, x => x.Codigo == "PRE_SOLICITUD");
        Assert.Equal(original.DocumentoVersionId, Assert.Single(vieja.ArchivosPresentados).DocumentoVersionId);
        var nueva = Assert.Single((await v.ObtenerRequisitosAsync(Actor(personas[1]), pre.PreregistroId, dos.PreregistroVersionId, default)).Items, x => x.Codigo == "PRE_SOLICITUD");
        Assert.Equal(corregida.DocumentoVersionId, Assert.Single(nueva.ArchivosPresentados).DocumentoVersionId);
        Assert.Equal(long.Parse(r1.PreregistroRequisitoId), (await db.Documentos.SingleAsync(x => x.DocumentoId == long.Parse(original.DocumentoId))).PreregistroRequisitoId);
    }
    [Fact]
    public async Task CambiarTiposConfiguradosNoAmpliaUnaDeterminacionHistorica()
    {
        await Catalogos(); var (personas, pre) = await Escenario();
        PreregistroVersionDto version; PreregistroRequisitoDto requisito;
        using (var normal = Servicios()) using (var scope = normal.CreateScope())
        {
            var s = scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>();
            version = await s.CrearAsync(Actor(personas[0]), pre.PreregistroId, Datos(), default);
            requisito = Assert.Single((await s.ObtenerRequisitosAsync(Actor(personas[0]), pre.PreregistroId, version.PreregistroVersionId, default)).Items, x => x.Codigo == "PRE_SOLICITUD");
        }
        using var cambiado = Servicios(extra: servicios => servicios.Configure<ArchivosPreregistroOpciones>(x =>
            x.TiposPorRequisito.Single(r => r.RequisitoCodigo == "PRE_SOLICITUD").TiposDocumentoCodigos.Add("DJ_SIN_HIJOS_MENORES")));
        using var otra = cambiado.CreateScope(); await using var db = postgres.CrearContexto();
        var tipo = await db.TiposDocumento.SingleAsync(x => x.Codigo == "DJ_SIN_HIJOS_MENORES");
        var bytes = PaqueteDPruebas.Pdf();
        await Assert.ThrowsAsync<ValidationException>(() => otra.ServiceProvider.GetRequiredService<IArchivosPreregistroServicio>()
            .CargarAsync(Actor(personas[0]), pre.PreregistroId, version.PreregistroVersionId, requisito.PreregistroRequisitoId, tipo.TipoDocumentoId,
                "Prueba no admitida", new(new MemoryStream(bytes), "archivo.pdf", bytes.Length), default));
        Assert.False(await db.Documentos.AnyAsync(x => x.ExpedienteId == long.Parse(pre.ExpedienteId)));
        Assert.Empty(Directory.Exists(raiz) ? Directory.GetFiles(raiz, "*", SearchOption.AllDirectories) : []);
    }
    [Fact]
    public async Task FaltaDeUnaAlternativaNoProduceUnaReglaIncompleta()
    {
        await Catalogos(); var (personas, pre) = await Escenario();
        using var servicios = Servicios(extra: x => x.Configure<ArchivosPreregistroOpciones>(o =>
            o.TiposPorRequisito.Single(r => r.RequisitoCodigo == "PRE_REGIMEN_MENORES").TiposDocumentoCodigos.Remove("ACTA_CONCILIACION_MENORES")));
        using var scope = servicios.CreateScope(); var s = scope.ServiceProvider.GetRequiredService<IVersionesPreregistroServicio>();
        var datos = Datos(); datos.Encuesta.CantidadHijosMenores = 2;
        var creada = await s.CrearAsync(Actor(personas[0]), pre.PreregistroId, datos, default);
        var r = await s.ObtenerRequisitosAsync(Actor(personas[0]), pre.PreregistroId, creada.PreregistroVersionId, default);
        Assert.DoesNotContain(r.Items, x => x.Codigo == "PRE_REGIMEN_MENORES");
        Assert.Contains(r.Items, x => x.Codigo == "PRE_NACIMIENTOS_MENORES");
        Assert.Contains("CONFIGURACION_REQUISITO_PRE_REGIMEN_MENORES", r.PendientesConfiguracion);
    }
    [Fact]
    public async Task RepositorioExcluyeCatalogosInactivosYTiposNoCiudadanos()
    {
        await using var db = postgres.CrearContexto(); var sufijo = Guid.NewGuid().ToString("N")[..10];
        var requisito = new RequisitoCatalogo { Codigo = "INACTIVO-" + sufijo, Nombre = "FICTICIO", Activo = false };
        TipoDocumento[] tipos = [new() { Codigo = "MUNI-" + sufijo, Nombre = "FICTICIO", OrigenCodigo = "MUNICIPALIDAD" },
            new() { Codigo = "INACT-" + sufijo, Nombre = "FICTICIO", OrigenCodigo = "CIUDADANO", Activo = false },
            new() { Codigo = "ACTIVO-" + sufijo, Nombre = "FICTICIO", OrigenCodigo = "AMBOS" }];
        db.RequisitosCatalogo.Add(requisito); db.TiposDocumento.AddRange(tipos); await db.SaveChangesAsync();
        var repositorio = new Divorcios.Datos.Repositorios.GeneracionRequisitosRepositorio(db);
        Assert.Empty(await repositorio.ObtenerCatalogosAsync([requisito.Codigo], default));
        Assert.Equal(tipos[2].Codigo, Assert.Single(await repositorio.ObtenerTiposAsync(tipos.Select(x => x.Codigo).ToArray(), default)).Codigo);
    }
    [Fact]
    public async Task LimiteDecimalDiezMbAceptaIgualYRechazaUnByteAdicional()
    {
        const int limite = 10000000;
        var longitudTexto = limite - PaqueteDPruebas.Pdf("").Length;
        var pdf = PaqueteDPruebas.Pdf(new string('x', longitudTexto));
        longitudTexto += limite - pdf.Length;
        pdf = PaqueteDPruebas.Pdf(new string('x', longitudTexto)); Assert.Equal(limite, pdf.Length);
        var almacen = new Divorcios.Datos.Almacenamiento.AlmacenamientoArchivosLocal(Microsoft.Extensions.Options.Options.Create(
            new AlmacenamientoArchivosOpciones { DirectorioRaiz = raiz }));
        using var entrada = new MemoryStream(pdf);
        var guardado = await almacen.GuardarNuevoAsync(entrada, limite, "application/pdf", default);
        Assert.Equal(limite, guardado.TamanoBytes);
        using var mayor = new MemoryStream(PaqueteDPruebas.Pdf(new string('x', longitudTexto + 1)));
        var error = await Assert.ThrowsAsync<Divorcios.Datos.Excepciones.ArchivoPersistenciaException>(() =>
            almacen.GuardarNuevoAsync(mayor, limite, "application/pdf", default));
        Assert.Equal("ARCHIVO_DEMASIADO_GRANDE", error.Codigo);
        Assert.Single(Directory.GetFiles(raiz, "*", SearchOption.AllDirectories));
    }
    [Fact]
    public async Task ContratoApiPublicaPdfDiezMbFormulariosYDeterminacionParcial()
    {
        await Catalogos(); var (personas, pre) = await Escenario();
        using var fabrica = new ApiAislada(postgres, x => Configurar(x));
        using var cliente = fabrica.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        var informacion = await cliente.GetFromJsonAsync<JsonElement>("/api/preregistro/informacion");
        Assert.Equal(10000000, informacion.GetProperty("politicaArchivos").GetProperty("maximoBytes").GetInt64());
        Assert.Equal("application/pdf", Assert.Single(informacion.GetProperty("politicaArchivos").GetProperty("mimePermitidos").EnumerateArray()).GetString());
        Assert.Equal(5, informacion.GetProperty("formatos").GetArrayLength());
        var login = await cliente.PostAsJsonAsync("/api/acceso-ciudadano/sesion", new { dni = personas[0].Dni, sufijoDireccion = "123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString());
        var respuesta = await cliente.PostAsJsonAsync($"/api/preregistros/{pre.PreregistroId}/versiones", Datos());
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var version = await respuesta.Content.ReadFromJsonAsync<PreregistroVersionDto>();
        var r = await cliente.GetFromJsonAsync<RequisitosVersionPreregistroDto>($"/api/preregistros/{pre.PreregistroId}/versiones/{version!.PreregistroVersionId}/requisitos");
        Assert.Equal("GENERACION_PARCIAL", r!.EstadoGeneracionCodigo); Assert.NotEmpty(r.PendientesConfiguracion);
        Assert.All(r.Items, x => Assert.NotNull(x.Determinacion)); Assert.Equal(0, fabrica.Proveedor.Llamadas);
    }
    public void Dispose()
    {
        // Retira exclusivamente los archivos ficticios de esta clase en su carpeta aleatoria verificada.
        var ruta = Path.GetFullPath(raiz); var temporal = Path.GetFullPath(Path.GetTempPath());
        if (!ruta.StartsWith(temporal, StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(ruta).StartsWith("divorcios-matriz-prueba-"))
            throw new InvalidOperationException("Ruta temporal de prueba no válida.");
        if (Directory.Exists(ruta)) Directory.Delete(ruta, recursive: true);
    }
}
