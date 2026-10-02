using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using Divorcios.Datos.Excepciones;
using Divorcios.Datos.Interfaces;
using Divorcios.Datos.Resultados;
using Divorcios.Dominio.Entidades;
using Divorcios.Negocio.DTOs.Comun;
using Divorcios.Negocio.DTOs.Preregistro;
using Divorcios.Negocio.Excepciones;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Opciones;
using Divorcios.Negocio.Resultados;
using Divorcios.Negocio.Validaciones;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Divorcios.Negocio.Servicios
{
    public sealed class ArchivosPreregistroServicio(IArchivosPreregistroRepositorio repositorio,
        IVersionesPreregistroRepositorio versiones, IPreregistrosRepositorio preregistros,
        IVersionesPreregistroServicio encuestas, IAlmacenamientoArchivos almacenamiento,
        IOptions<InformacionPreregistroOpciones> politica, IOptions<ArchivosPreregistroOpciones> reglas,
        TimeProvider reloj, ILogger<ArchivosPreregistroServicio> logger) : IArchivosPreregistroServicio
    {
        public async Task<DocumentoVersionDto> CargarAsync(ActorCiudadano actor, string preregistroId, string versionId,
            string requisitoId, short tipoDocumentoId, string titulo, ArchivoEntrada archivo, CancellationToken cancellationToken)
        {
            var p = Leer(preregistroId); var v = Leer(versionId); var r = Leer(requisitoId);
            if (tipoDocumentoId <= 0) throw new ValidationException("El tipo de documento debe ser positivo.");
            titulo = TextoObligatorio(titulo, 200, "título");
            return await EjecutarCargaAsync(p, actor, async guardar =>
            {
                var (lectura, requisito) = await EditableAsync(actor, p, v, r, cancellationToken);
                var tipo = await repositorio.ObtenerTipoAsync(tipoDocumentoId, cancellationToken)
                    ?? throw new ValidationException("Tipo de documento no encontrado.");
                ValidarTipo(requisito, tipo, lectura);
                var nombre = NombreSeguro(archivo.NombreArchivo);
                var almacenado = await guardar(archivo, MimePorExtension(nombre));
                var ahora = Ahora();
                var documento = new Documento
                {
                    ExpedienteId = lectura.Cabecera.Registro.ExpedienteId, PreregistroRequisitoId = r,
                    TipoDocumentoId = tipoDocumentoId, Titulo = titulo, EtapaCodigo = "PRERREGISTRO",
                    EstadoCodigo = "VIGENTE", CreadoPorCuentaId = actor.CuentaCiudadanaId, CreadoEn = ahora
                };
                var version = NuevaVersion(actor, nombre, almacenado, ahora);
                version.Documento = documento;
                await repositorio.AgregarAsync(version, cancellationToken);
                var antes = requisito.ArchivosPresentados.Select(x => x.DocumentoVersionId).ToArray();
                await repositorio.SeleccionarAsync(requisito, antes.Append(version.DocumentoVersionId).ToArray(), cancellationToken);
                await AuditarAsync(actor, documento.ExpedienteId, r, "DOCUMENTO_CARGADO", antes,
                    antes.Append(version.DocumentoVersionId).ToArray(), null, cancellationToken);
                return Mapear(preregistroId, documento, version);
            }, cancellationToken);
        }

        public async Task<DocumentoVersionDto> CorregirAsync(ActorCiudadano actor, string preregistroId, string versionId,
            string requisitoId, string documentoId, string documentoVersionBaseId, string motivoCambio, ArchivoEntrada archivo, CancellationToken cancellationToken)
        {
            var p = Leer(preregistroId); var v = Leer(versionId); var r = Leer(requisitoId);
            var d = Leer(documentoId); var baseId = Leer(documentoVersionBaseId);
            var motivo = TextoObligatorio(motivoCambio, 300, "motivo del cambio");
            return await EjecutarCargaAsync(p, actor, async guardar =>
            {
                var (lectura, requisito) = await EditableAsync(actor, p, v, r, cancellationToken);
                var documento = await repositorio.ObtenerDocumentoAsync(lectura.Cabecera.Registro.ExpedienteId, d, cancellationToken)
                    ?? throw NoEncontrado();
                if (!requisito.ArchivosPresentados.Any(x => x.DocumentoVersion.DocumentoId == d))
                    throw NoEncontrado();
                if (documento.EstadoCodigo != "VIGENTE")
                    throw new ConflictoNegocioException("El documento no admite nuevas versiones.");
                var ultima = documento.Versiones.OrderByDescending(x => x.NumeroVersion).FirstOrDefault();
                if (ultima?.DocumentoVersionId != baseId)
                    throw new ConflictoNegocioException("La versión base del documento cambió. Consulte sus archivos antes de corregir.");
                if (ultima.NumeroVersion == int.MaxValue)
                    throw new ConflictoNegocioException("Se alcanzó el máximo de versiones del documento.");
                ValidarTipo(requisito, documento.TipoDocumento, lectura);
                var nombre = NombreSeguro(archivo.NombreArchivo);
                var almacenado = await guardar(archivo, MimePorExtension(nombre));
                var version = NuevaVersion(actor, nombre, almacenado, Ahora());
                version.DocumentoId = d; version.NumeroVersion = ultima.NumeroVersion + 1; version.MotivoCambio = motivo;
                await repositorio.AgregarAsync(version, cancellationToken);
                var antes = requisito.ArchivosPresentados.Select(x => x.DocumentoVersionId).ToArray();
                var despues = requisito.ArchivosPresentados.Where(x => x.DocumentoVersion.DocumentoId != d)
                    .Select(x => x.DocumentoVersionId).Append(version.DocumentoVersionId).ToArray();
                await repositorio.SeleccionarAsync(requisito, despues, cancellationToken);
                await AuditarAsync(actor, documento.ExpedienteId, r, "DOCUMENTO_CORREGIDO", antes, despues, motivo, cancellationToken);
                return Mapear(preregistroId, documento, version);
            }, cancellationToken);
        }

        public async Task<PreregistroRequisitoDto> SeleccionarAsync(ActorCiudadano actor, string preregistroId, string versionId,
            string requisitoId, SeleccionarArchivosPreregistroDto datos, CancellationToken cancellationToken)
        {
            var p = Leer(preregistroId); var v = Leer(versionId); var r = Leer(requisitoId);
            Validator.ValidateObject(datos, new ValidationContext(datos), true);
            var ids = datos.DocumentoVersionIds.Select(Leer).ToArray();
            if (ids.Distinct().Count() != ids.Length) throw new ValidationException("No repita versiones de archivo.");
            return await versiones.EjecutarEscrituraAsync(p, async () =>
            {
                var (lectura, requisito) = await EditableAsync(actor, p, v, r, cancellationToken);
                var actuales = requisito.ArchivosPresentados.Select(x => x.DocumentoVersionId).ToArray();
                var candidatas = await repositorio.ObtenerVersionesAsync(lectura.Cabecera.Registro.ExpedienteId, ids, cancellationToken);
                if (candidatas.Count != ids.Length) throw NoEncontrado();
                if (candidatas.Select(x => x.DocumentoId).Distinct().Count() != candidatas.Count)
                    throw new ValidationException("Seleccione como máximo una versión de cada documento para este requisito.");
                foreach (var candidata in candidatas.Where(x => !actuales.Contains(x.DocumentoVersionId)))
                {
                    if (candidata.Documento.EstadoCodigo != "VIGENTE")
                        throw new ConflictoNegocioException("El documento seleccionado no está vigente.");
                    ValidarTipo(requisito, candidata.Documento.TipoDocumento, lectura);
                    // Comprueba disponibilidad e integridad antes de aceptar una reutilización.
                    try
                    {
                        await using var comprobacion = await almacenamiento.AbrirVerificadoAsync(candidata.AlmacenamientoClave,
                            candidata.Sha256, candidata.TamanoBytes, candidata.MimeType, cancellationToken);
                    }
                    catch (ArchivoPersistenciaException e) { throw Traducir(e); }
                }
                if (!actuales.ToHashSet().SetEquals(ids))
                {
                    await repositorio.SeleccionarAsync(requisito, ids, cancellationToken);
                    await AuditarAsync(actor, lectura.Cabecera.Registro.ExpedienteId, r, "ARCHIVOS_PRESENTADOS_SELECCIONADOS",
                        actuales, ids, null, cancellationToken);
                }
                var respuesta = await encuestas.ObtenerRequisitosAsync(actor, preregistroId, versionId, cancellationToken);
                return respuesta.Items.Single(x => x.PreregistroRequisitoId == requisitoId);
            }, cancellationToken);
        }

        public async Task<PaginaDto<DocumentoPreregistroDto>> ListarAsync(ActorCiudadano actor, string preregistroId,
            ListarDocumentosPreregistroDto filtro, CancellationToken cancellationToken)
        {
            var p = Leer(preregistroId);
            Validator.ValidateObject(filtro, new ValidationContext(filtro), true);
            await ValidarActorAsync(actor, cancellationToken);
            await CabeceraVisibleAsync(p, actor.PersonaId, cancellationToken);
            var pagina = await repositorio.ListarAsync(p, actor.PersonaId, filtro.TipoDocumentoId, filtro.Pagina, filtro.TamanoPagina, cancellationToken);
            if (pagina.Items.Any(d =>
                (d.PreregistroRequisito is not null && d.PreregistroRequisito.PreregistroVersion.PreregistroId != p)
                || d.Versiones.Any(a => a.PresentacionesPreregistro.Any(x => x.PreregistroRequisito.PreregistroVersion.PreregistroId != p)
                    || a.Evaluaciones.Any(x => x.RevisionDetalle.RevisionPreregistro.PreregistroVersion.PreregistroId != p))))
                throw new ConflictoNegocioException("Las asociaciones históricas de los archivos requieren revisión de integridad.");
            return new(pagina.Items.Select(d => new DocumentoPreregistroDto(Id(d.DocumentoId), d.TipoDocumentoId,
                d.TipoDocumento.Codigo, d.TipoDocumento.Activo, d.Titulo, d.EstadoCodigo,
                d.PreregistroRequisitoId.HasValue ? Id(d.PreregistroRequisitoId.Value) : null,
                d.Versiones.OrderByDescending(x => x.NumeroVersion).Select(a => new DocumentoVersionListadoDto(Mapear(preregistroId, d, a),
                    a.PresentacionesPreregistro.OrderBy(x => x.PreregistroRequisitoId).Select(x =>
                        new PresentacionArchivoDto(Id(x.PreregistroRequisito.PreregistroVersionId), Id(x.PreregistroRequisitoId))).ToArray(),
                    a.Evaluaciones.OrderBy(x => x.RevisionDetalleId).Select(x =>
                        new EvaluacionArchivoDto(Id(x.RevisionDetalle.RevisionPreregistroId), Id(x.RevisionDetalleId), x.RevisionDetalle.ResultadoCodigo)).ToArray()
                )).ToArray())).ToArray(), filtro.Pagina, filtro.TamanoPagina, pagina.Total);
        }

        public async Task<DescargaArchivo> DescargarAsync(ActorCiudadano actor, string preregistroId, string documentoId,
            string documentoVersionId, CancellationToken cancellationToken)
        {
            var p = Leer(preregistroId); var d = Leer(documentoId); var a = Leer(documentoVersionId);
            await ValidarActorAsync(actor, cancellationToken);
            var archivo = await repositorio.ObtenerArchivoAsync(p, actor.PersonaId, d, a, cancellationToken) ?? throw NoEncontrado();
            var nombre = NombreSeguro(archivo.NombreArchivo);
            try
            {
                var contenido = await almacenamiento.AbrirVerificadoAsync(archivo.AlmacenamientoClave, archivo.Sha256,
                    archivo.TamanoBytes, archivo.MimeType, cancellationToken);
                return new(contenido, nombre, archivo.MimeType);
            }
            catch (ArchivoPersistenciaException e) { throw Traducir(e); }
        }

        private async Task<DocumentoVersionDto> EjecutarCargaAsync(long preregistroId, ActorCiudadano actor,
            Func<Func<ArchivoEntrada, string, Task<ArchivoAlmacenado>>, Task<DocumentoVersionDto>> operacion, CancellationToken cancellationToken)
        {
            ArchivoAlmacenado? nuevo = null;
            try
            {
                return await versiones.EjecutarEscrituraAsync(preregistroId, () => operacion(async (entrada, mime) =>
                {
                    var maximo = ValidarPolitica(mime);
                    if (entrada.LongitudDeclarada == 0) throw new ArchivoNegocioException(400, "ARCHIVO_VACIO", "Debe adjuntar un archivo con contenido.");
                    if (entrada.LongitudDeclarada > maximo)
                        throw new ArchivoNegocioException(413, "ARCHIVO_DEMASIADO_GRANDE", "El archivo supera el tamaño permitido.");
                    nuevo = await almacenamiento.GuardarNuevoAsync(entrada.Contenido, maximo, mime, cancellationToken);
                    return nuevo;
                }), cancellationToken);
            }
            catch (Exception e)
            {
                if (nuevo is not null)
                {
                    // Un error al confirmar puede tener resultado incierto: nunca borrar antes de comprobar referencias.
                    using var limite = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    try
                    {
                        if (!await repositorio.EstaClavePersistidaAsync(nuevo.Clave, limite.Token))
                            await almacenamiento.DescartarNuevoAsync(nuevo.Clave, limite.Token);
                    }
                    catch
                    {
                        logger.LogWarning("Compensación de archivo pendiente de conciliación. Correlación {CorrelacionId}.", actor.CorrelacionId);
                    }
                }
                if (e is ArchivoPersistenciaException persistencia) throw Traducir(persistencia);
                throw;
            }
        }

        private async Task<(LecturaVersionesPreregistro Lectura, PreregistroRequisito Requisito)> EditableAsync(
            ActorCiudadano actor, long preregistroId, long versionId, long requisitoId, CancellationToken cancellationToken)
        {
            await ValidarActorAsync(actor, cancellationToken);
            var lectura = await versiones.LeerAsync(preregistroId, actor.PersonaId, versionId, false, cancellationToken) ?? throw NoEncontrado();
            var requisito = await repositorio.ObtenerRequisitoAsync(versionId, requisitoId, cancellationToken) ?? throw NoEncontrado();
            if (requisito.ArchivosPresentados.Any(x => x.DocumentoVersion.Documento.ExpedienteId != lectura.Cabecera.Registro.ExpedienteId
                || x.DocumentoVersion.Documento.EtapaCodigo != "PRERREGISTRO"))
                throw new ConflictoNegocioException("Los archivos presentados requieren revisión de integridad.");
            if (!lectura.Cabecera.Registro.Expediente.Conyuges.Any(x => x.PersonaId == actor.PersonaId && x.EsIniciador))
                throw new AccesoDenegadoNegocioException("Sólo el iniciador puede modificar archivos presentados.");
            if (!ReglasEdicionPreregistro.EsVersionTrabajo(lectura, versionId) || requisito.DetallesRevision.Count > 0)
                throw new ConflictoNegocioException("Esta versión no admite cambios de archivos. Prepare una nueva encuesta cuando corresponda.");
            return (lectura, requisito);
        }

        private async Task ValidarActorAsync(ActorCiudadano actor, CancellationToken cancellationToken)
        {
            var cuenta = await preregistros.ObtenerCuentaAsync(actor.CuentaCiudadanaId, cancellationToken);
            if (actor.PersonaId <= 0 || actor.CuentaCiudadanaId <= 0 || cuenta is null || cuenta.PersonaId != actor.PersonaId
                || cuenta.EstadoCodigo != "ACTIVA" || cuenta.BloqueadoHasta > reloj.GetUtcNow().UtcDateTime)
                throw new AccesoCiudadanoRechazadoException();
        }
        private async Task<CabeceraPreregistro> CabeceraVisibleAsync(long id, long personaId, CancellationToken cancellationToken)
            => await preregistros.ObtenerAsync(id, personaId, cancellationToken) ?? throw NoEncontrado();

        private void ValidarTipo(PreregistroRequisito requisito, TipoDocumento tipo, LecturaVersionesPreregistro lectura)
        {
            if (!requisito.Aplica || requisito.EstadoCodigo == "NO_APLICA")
                throw new ConflictoNegocioException("El requisito no admite presentación de archivos.");
            if (!requisito.RequisitoCatalogo.Activo || !tipo.Activo || tipo.OrigenCodigo is not ("CIUDADANO" or "AMBOS"))
                throw new ConflictoNegocioException("El requisito o tipo no está habilitado para cargas ciudadanas.");
            var correspondencias = reglas.Value.TiposPorRequisito
                .Where(x => x.Confirmada && x.RequisitoCodigo == requisito.RequisitoCatalogo.Codigo).ToArray();
            if (correspondencias.Length != 1 || correspondencias[0].TiposDocumentoCodigos.Count == 0
                || correspondencias[0].TiposDocumentoCodigos.Any(string.IsNullOrWhiteSpace))
                throw new ServicioNoDisponibleNegocioException("TIPOS_REQUISITO_PENDIENTES",
                    "La correspondencia entre este requisito y los tipos de documento sigue pendiente de configuración validada.");
            if (!correspondencias[0].TiposDocumentoCodigos.Contains(tipo.Codigo, StringComparer.Ordinal))
                throw new ValidationException("El tipo de documento no corresponde a este requisito.");
            var generacion = EvidenciaGeneracionRequisitos.Leer(lectura, requisito.PreregistroVersionId);
            var snapshot = generacion.RequisitosGenerados
                .SingleOrDefault(x => x.PreregistroRequisitoId == Id(requisito.PreregistroRequisitoId))?.Definicion;
            if ((generacion.EstadoCodigo == "GENERACION_PARCIAL" && snapshot is null)
                || (generacion.EstadoCodigo == "SIN_ACREDITAR" && requisito.RequisitoCatalogo.Codigo.StartsWith("PRE_", StringComparison.Ordinal)))
                throw new ConflictoNegocioException("La determinación histórica del requisito requiere revisión de integridad.");
            if (snapshot is not null && (snapshot.Codigo != requisito.RequisitoCatalogo.Codigo
                || !snapshot.TiposDocumentoCodigos.Contains(tipo.Codigo, StringComparer.Ordinal)))
                throw new ValidationException("El tipo no corresponde a la determinación conservada de este requisito.");
        }

        private long ValidarPolitica(string mime)
        {
            var configuracion = politica.Value;
            var soportados = new[] { "application/pdf", "image/jpeg", "image/png" };
            if (configuracion.MaximoBytes is not > 0 || configuracion.MaximoBytes > long.MaxValue - 65536
                || configuracion.MimePermitidos.Count == 0 || configuracion.MimePermitidos.Any(x => !soportados.Contains(x)))
                throw new ServicioNoDisponibleNegocioException("POLITICA_ARCHIVOS_PENDIENTE", "La política de carga de archivos no está configurada.");
            if (!configuracion.MimePermitidos.Contains(mime))
                throw new ArchivoNegocioException(415, "CONTENIDO_NO_PERMITIDO", "El formato del archivo no está permitido.");
            return configuracion.MaximoBytes.Value;
        }

        private async Task AuditarAsync(ActorCiudadano actor, long expedienteId, long requisitoId, string accion,
            IReadOnlyList<long> antes, IReadOnlyList<long> despues, string? motivo, CancellationToken cancellationToken)
            => await preregistros.RegistrarAuditoriaAsync(new()
            {
                ActorTipoCodigo = "CUENTA_CIUDADANA", CuentaCiudadanaId = actor.CuentaCiudadanaId,
                ExpedienteId = expedienteId, AccionCodigo = accion, RecursoCodigo = "PREREGISTRO_REQUISITO", RecursoId = Id(requisitoId),
                ResultadoCodigo = "EXITO", Descripcion = "Cambio de archivos presentados en la versión de trabajo.",
                DetalleJson = JsonSerializer.Serialize(new { antes = antes.Select(Id), despues = despues.Select(Id), motivo }),
                CorrelacionId = actor.CorrelacionId == Guid.Empty ? Guid.NewGuid() : actor.CorrelacionId, RegistradoEn = Ahora(),
                DireccionIp = Recortar(actor.DireccionIp, 64), UserAgent = Recortar(actor.UserAgent, 500)
            }, cancellationToken);

        private static DocumentoVersion NuevaVersion(ActorCiudadano actor, string nombre, ArchivoAlmacenado archivo, DateTime creadoEn)
            => new() { NumeroVersion = 1, NombreArchivo = nombre, MimeType = archivo.MimeType, TamanoBytes = archivo.TamanoBytes,
                AlmacenamientoClave = archivo.Clave, Sha256 = archivo.Sha256, CreadoEn = creadoEn, CargadoPorCuentaId = actor.CuentaCiudadanaId };
        private static DocumentoVersionDto Mapear(string preregistroId, Documento d, DocumentoVersion v)
            => new(Id(d.DocumentoId), Id(v.DocumentoVersionId), v.NumeroVersion, d.TipoDocumentoId, d.Titulo,
                v.NombreArchivo, v.MimeType, v.TamanoBytes, v.Sha256, v.CreadoEn,
                $"/api/preregistros/{preregistroId}/documentos/{Id(d.DocumentoId)}/versiones/{Id(v.DocumentoVersionId)}/archivo");
        private static string NombreSeguro(string? nombre)
        {
            var seguro = Path.GetFileName((nombre ?? "").Replace('\\', '/')).Trim();
            if (seguro.Length is 0 or > 240 || seguro.Any(c => char.IsControl(c) || Path.GetInvalidFileNameChars().Contains(c)))
                throw new ValidationException("El nombre de archivo no es válido.");
            return seguro;
        }
        private static string MimePorExtension(string nombre) => Path.GetExtension(nombre).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            _ => throw new ArchivoNegocioException(415, "CONTENIDO_NO_PERMITIDO", "La extensión del archivo no está permitida.")
        };
        private static Exception Traducir(ArchivoPersistenciaException e) => e.Codigo switch
        {
            "ARCHIVO_VACIO" => new ArchivoNegocioException(400, e.Codigo, "Debe adjuntar un archivo con contenido."),
            "ARCHIVO_DEMASIADO_GRANDE" => new ArchivoNegocioException(413, e.Codigo, "El archivo supera el tamaño permitido."),
            "CONTENIDO_NO_PERMITIDO" => new ArchivoNegocioException(415, e.Codigo, "El contenido no corresponde al formato admitido."),
            _ => new ServicioNoDisponibleNegocioException("ARCHIVO_NO_DISPONIBLE", "El archivo no está disponible o no supera la comprobación de integridad.")
        };
        private DateTime Ahora() => reloj.GetUtcNow().UtcDateTime;
        private static long Leer(string id) => IdentificadorPositivoAttribute.Leer(id);
        private static string Id(long id) => id.ToString(CultureInfo.InvariantCulture);
        private static string TextoObligatorio(string? valor, int maximo, string campo)
        {
            var texto = valor?.Trim();
            if (string.IsNullOrEmpty(texto) || texto.Length > maximo) throw new ValidationException($"Indique un {campo} válido, de hasta {maximo} caracteres.");
            return texto;
        }
        private static string? Recortar(string? valor, int maximo) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim()[..Math.Min(valor.Trim().Length, maximo)];
        private static RecursoNoEncontradoNegocioException NoEncontrado() => new("Prerregistro, requisito o archivo no encontrados.");
    }
}
