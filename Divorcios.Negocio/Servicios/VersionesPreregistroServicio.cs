using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using Divorcios.Datos.Interfaces;
using Divorcios.Datos.Resultados;
using Divorcios.Dominio.Entidades;
using Divorcios.Negocio.DTOs.Preregistro;
using Divorcios.Negocio.Excepciones;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Resultados;
using Divorcios.Negocio.Validaciones;

namespace Divorcios.Negocio.Servicios
{
    public sealed class VersionesPreregistroServicio(IVersionesPreregistroRepositorio repositorio,
        IPreregistrosRepositorio preregistros, IGeneracionRequisitosServicio generador, TimeProvider reloj) : IVersionesPreregistroServicio
    {

        public async Task<PreregistroVersionDto> CrearAsync(ActorCiudadano actor, string preregistroId,
            CrearVersionPreregistroDto datos, CancellationToken cancellationToken)
        {
            var id = IdentificadorPositivoAttribute.Leer(preregistroId);
            ValidarDatos(datos);
            var baseId = datos.VersionBaseId is null ? (long?)null : IdentificadorPositivoAttribute.Leer(datos.VersionBaseId);
            return await repositorio.EjecutarEscrituraAsync(id, async () =>
            {
                await ValidarActorAsync(actor, cancellationToken);
                var cabecera = await preregistros.ObtenerAsync(id, actor.PersonaId, cancellationToken)
                    ?? throw NoEncontrado();
                if (!cabecera.Registro.Expediente.Conyuges.Any(x => x.PersonaId == actor.PersonaId && x.EsIniciador))
                    throw new AccesoDenegadoNegocioException("Sólo el iniciador puede crear una versión.");
                if (!ReglasEdicionPreregistro.PermiteNuevaVersion(cabecera))
                    throw new ConflictoNegocioException("El prerregistro no permite crear versiones en su estado actual.");
                var ultima = cabecera.Versiones.FirstOrDefault();
                if (baseId != ultima?.VersionId)
                    throw new ConflictoNegocioException("La versión base no es la vigente. Consulte nuevamente antes de guardar.");
                var motivo = Texto(datos.MotivoCambio);
                if (ultima is not null && motivo is null)
                    throw new ValidationException("Desde la segunda versión debe indicar el motivo del cambio.");
                if (ultima?.NumeroVersion == int.MaxValue)
                    throw new ConflictoNegocioException("Se alcanzó el máximo de versiones admitido.");

                var actuales = await repositorio.ObtenerContactosActualesAsync(cabecera.Registro.ExpedienteId, cancellationToken);
                var anterior = ultima is null ? cabecera.Registro.CreadoEn :
                    (await repositorio.LeerAsync(id, actor.PersonaId, ultima.VersionId, false, cancellationToken))!.Versiones[0].CreadoEn;
                // PostgreSQL almacena microsegundos. La frontera debe ser estrictamente posterior para reconstruir [desde, hasta).
                var frontera = actuales.Select(x => x.VigenteDesde).Append(anterior).Append(reloj.GetUtcNow().UtcDateTime).Max();
                var ahora = new DateTime(frontera.Ticks - frontera.Ticks % 10 + 10, DateTimeKind.Utc);
                var e = datos.Encuesta;
                var version = new PreregistroVersion
                {
                    PreregistroId = id, NumeroVersion = (ultima?.NumeroVersion ?? 0) + 1, CreadoPorCuentaId = actor.CuentaCiudadanaId,
                    CreadoEn = ahora, MotivoCambio = motivo, FechaMatrimonio = e.FechaMatrimonio!.Value,
                    MatrimonioEnPorvenir = e.MatrimonioEnPorvenir!.Value, UltimoDomicilioConyugalPorvenir = e.UltimoDomicilioConyugalPorvenir!.Value,
                    DomicilioConyugal = Texto(e.DomicilioConyugal), TieneHijos = e.TieneHijos!.Value,
                    CantidadHijosMenores = (short)e.CantidadHijosMenores!.Value, CantidadHijosMayores = (short)e.CantidadHijosMayores!.Value,
                    TieneHijosMayoresSituacionEspecial = e.TieneHijosMayoresSituacionEspecial,
                    TieneBienes = e.TieneBienes!.Value, TieneAcuerdoBienes = e.TieneAcuerdoBienes!.Value,
                    RequiereRepresentacionA = e.RequiereRepresentacionA, RequiereRepresentacionB = e.RequiereRepresentacionB,
                    ObservacionCiudadano = Texto(e.ObservacionCiudadano)
                };
                await repositorio.AgregarVersionAsync(version, cancellationToken);
                var generacion = await generador.GenerarAsync(version, cancellationToken);
                List<ExpedienteContactoHistorial> nuevos = [];
                foreach (var contacto in datos.Contactos)
                {
                    var conyuge = cabecera.Registro.Expediente.Conyuges.Single(x => x.PosicionCodigo == contacto.PosicionCodigo);
                    foreach (var (tipo, valor) in Valores(contacto))
                    {
                        var previo = actuales.SingleOrDefault(x => x.ExpedienteConyugeId == conyuge.ExpedienteConyugeId && x.TipoContactoCodigo == tipo);
                        if (previo?.Valor == valor) continue;
                        if (previo is not null) previo.VigenteHasta = ahora;
                        if (valor is not null)
                            nuevos.Add(new()
                            {
                                ExpedienteConyugeId = conyuge.ExpedienteConyugeId, TipoContactoCodigo = tipo, Valor = valor,
                                FuenteCodigo = "PRERREGISTRO", PreregistroVersionOrigenId = version.PreregistroVersionId, VigenteDesde = ahora
                            });
                    }
                }
                // Cerrar primero libera el índice único de contactos vigentes; ambos pasos están en la misma transacción.
                await repositorio.GuardarAsync(cancellationToken);
                await repositorio.AgregarContactosAsync(nuevos, cancellationToken);
                await preregistros.RegistrarAuditoriaAsync(new RegistroAuditoria
                {
                    ActorTipoCodigo = "CUENTA_CIUDADANA", CuentaCiudadanaId = actor.CuentaCiudadanaId,
                    ExpedienteId = cabecera.Registro.ExpedienteId, AccionCodigo = "PREREGISTRO_VERSION_CREADA",
                    RecursoCodigo = "PREREGISTRO_VERSION", RecursoId = Id(version.PreregistroVersionId),
                    ResultadoCodigo = "EXITO", Descripcion = "Encuesta, contactos y determinación de requisitos guardados con sus pendientes.",
                    DetalleJson = JsonSerializer.Serialize(new
                    {
                        versionBaseId = baseId.HasValue ? Id(baseId.Value) : null, numeroVersion = version.NumeroVersion,
                        estadoGeneracionRequisitosCodigo = generacion.EstadoCodigo,
                        matrizVersion = generacion.MatrizVersion,
                        pendientesGeneracionRequisitos = generacion.Pendientes,
                        requisitosGenerados = generacion.RequisitosGenerados
                    }),
                    RegistradoEn = ahora, CorrelacionId = actor.CorrelacionId == Guid.Empty ? Guid.NewGuid() : actor.CorrelacionId,
                    DireccionIp = Recortar(actor.DireccionIp, 64), UserAgent = Recortar(actor.UserAgent, 500)
                }, cancellationToken);
                var lectura = await repositorio.LeerAsync(id, actor.PersonaId, version.PreregistroVersionId, false, cancellationToken)
                    ?? throw new InvalidOperationException("No se pudo recuperar la versión creada.");
                return Mapear(lectura, actor.PersonaId);
            }, cancellationToken);
        }

        public async Task<IReadOnlyList<PreregistroVersionResumenDto>> ListarAsync(ActorCiudadano actor,
            string preregistroId, CancellationToken cancellationToken)
        {
            var lectura = await LeerAsync(actor, preregistroId, null, false, cancellationToken);
            return lectura.Versiones.Select(v =>
            {
                var revisiones = lectura.Cabecera.Revisiones.Where(x => x.VersionId == v.PreregistroVersionId).ToArray();
                var creada = EventoCreacion(lectura, v.PreregistroVersionId);
                var enviada = lectura.Eventos.Any(x => x.RecursoId == Id(v.PreregistroVersionId) && x.AccionCodigo == "PREREGISTRO_ENVIADO")
                    || revisiones.Length > 0;
                bool? estadoEnvio = enviada ? true : creada is not null
                    && (lectura.Cabecera.Registro.EnviadoEn is null || lectura.Cabecera.Registro.EnviadoEn < v.CreadoEn) ? false : null;
                return new PreregistroVersionResumenDto(Id(v.PreregistroVersionId), v.NumeroVersion, v.CreadoEn, v.MotivoCambio,
                    ReglasEdicionPreregistro.EsVersionTrabajo(lectura, v.PreregistroVersionId), estadoEnvio,
                    revisiones.OrderByDescending(x => x.IniciadaEn).ThenByDescending(x => x.RevisionId)
                        .Select(x => new RevisionVersionReferenciaDto(Id(x.RevisionId), x.ResultadoCodigo, x.IniciadaEn, x.FinalizadaEn)).ToArray(),
                    EstadoGeneracion(lectura, v.PreregistroVersionId));
            }).ToArray();
        }

        public async Task<PreregistroVersionDto> ObtenerAsync(ActorCiudadano actor, string preregistroId,
            string versionId, CancellationToken cancellationToken)
            => Mapear(await LeerAsync(actor, preregistroId, versionId, false, cancellationToken), actor.PersonaId);

        public async Task<RequisitosVersionPreregistroDto> ObtenerRequisitosAsync(ActorCiudadano actor, string preregistroId,
            string versionId, CancellationToken cancellationToken)
        {
            var lectura = await LeerAsync(actor, preregistroId, versionId, true, cancellationToken);
            var v = lectura.Versiones.Single();
            var generacion = EvidenciaGeneracionRequisitos.Leer(lectura, v.PreregistroVersionId);
            // Consultar asociaciones exactas, nunca Documento.Versiones.Last ni la FK de requisito de origen.
            var items = v.Requisitos.OrderBy(x => x.RequisitoCatalogo.Codigo).ThenBy(x => x.PreregistroRequisitoId).Select(r =>
                new PreregistroRequisitoDto(Id(r.PreregistroRequisitoId), r.RequisitoCatalogoId, r.RequisitoCatalogo.Codigo,
                    r.RequisitoCatalogo.Nombre, r.RequisitoCatalogo.Descripcion, r.Aplica, r.Obligatorio, r.EstadoCodigo,
                    r.ArchivosPresentados.OrderBy(x => x.DocumentoVersionId).Select(a =>
                    {
                        var archivo = a.DocumentoVersion;
                        if (archivo.Documento.ExpedienteId != lectura.Cabecera.Registro.ExpedienteId)
                            throw new ConflictoNegocioException("La evidencia presentada contiene una asociación inconsistente.");
                        return new ArchivoPresentadoPreregistroDto(Id(archivo.DocumentoId), Id(archivo.DocumentoVersionId),
                            archivo.NumeroVersion, archivo.NombreArchivo, archivo.MimeType, archivo.TamanoBytes, archivo.Sha256, archivo.CreadoEn);
                    }).ToArray(), Determinacion(r, generacion))).ToArray();
            // La existencia de registros tampoco acredita una matriz completa validada.
            return new(Id(v.PreregistroVersionId), generacion.EstadoCodigo, generacion.Pendientes, items);
        }

        private async Task<LecturaVersionesPreregistro> LeerAsync(ActorCiudadano actor, string preregistroId,
            string? versionId, bool requisitos, CancellationToken cancellationToken)
        {
            var id = IdentificadorPositivoAttribute.Leer(preregistroId);
            var version = versionId is null ? (long?)null : IdentificadorPositivoAttribute.Leer(versionId);
            await ValidarActorAsync(actor, cancellationToken);
            return await repositorio.LeerAsync(id, actor.PersonaId, version, requisitos, cancellationToken) ?? throw NoEncontrado();
        }

        private async Task ValidarActorAsync(ActorCiudadano actor, CancellationToken cancellationToken)
        {
            var cuenta = await preregistros.ObtenerCuentaAsync(actor.CuentaCiudadanaId, cancellationToken);
            if (actor.CuentaCiudadanaId <= 0 || actor.PersonaId <= 0 || cuenta is null || cuenta.PersonaId != actor.PersonaId
                || cuenta.EstadoCodigo != "ACTIVA" || cuenta.BloqueadoHasta > reloj.GetUtcNow().UtcDateTime)
                throw new AccesoCiudadanoRechazadoException();
        }

        private static PreregistroVersionDto Mapear(LecturaVersionesPreregistro lectura, long personaId)
        {
            var v = lectura.Versiones.Single();
            var contactos = lectura.Cabecera.Registro.Expediente.Conyuges.OrderBy(x => x.PosicionCodigo).Select(c =>
            {
                var vigentes = lectura.Contactos.Where(x => x.ExpedienteConyugeId == c.ExpedienteConyugeId).ToArray();
                if (vigentes.GroupBy(x => x.TipoContactoCodigo).Any(x => x.Count() > 1))
                    throw new ConflictoNegocioException("El historial contiene contactos con vigencias superpuestas.");
                string? Valor(string tipo) => vigentes.SingleOrDefault(x => x.TipoContactoCodigo == tipo)?.Valor;
                return new ContactoPreregistroDto { PosicionCodigo = c.PosicionCodigo, Celular = Valor("CELULAR"),
                    Correo = Valor("CORREO"), Direccion = Valor("DIRECCION") };
            }).ToArray();
            return new(Id(v.PreregistroId), Id(v.PreregistroVersionId), v.NumeroVersion, v.CreadoEn, v.MotivoCambio,
                new(v.FechaMatrimonio, v.MatrimonioEnPorvenir, v.UltimoDomicilioConyugalPorvenir, v.DomicilioConyugal,
                    v.TieneHijos, v.CantidadHijosMenores, v.CantidadHijosMayores, v.TieneHijosMayoresSituacionEspecial,
                    v.TieneBienes, v.TieneAcuerdoBienes, v.RequiereRepresentacionA, v.RequiereRepresentacionB, v.ObservacionCiudadano),
                contactos, ReglasEdicionPreregistro.EsVersionTrabajo(lectura, v.PreregistroVersionId)
                    && lectura.Cabecera.Registro.Expediente.Conyuges.Any(x => x.PersonaId == personaId && x.EsIniciador),
                EstadoGeneracion(lectura, v.PreregistroVersionId));
        }

        private static RegistroAuditoria? EventoCreacion(LecturaVersionesPreregistro lectura, long id)
            => lectura.Eventos.Where(x => x.RecursoId == Id(id) && x.AccionCodigo == "PREREGISTRO_VERSION_CREADA")
                .OrderBy(x => x.RegistroAuditoriaId).FirstOrDefault();

        private static string EstadoGeneracion(LecturaVersionesPreregistro lectura, long id)
        {
            return EvidenciaGeneracionRequisitos.Leer(lectura, id).EstadoCodigo;
        }

        private static DeterminacionRequisitoDto? Determinacion(PreregistroRequisito requisito, GeneracionRequisitosResultado generacion)
        {
            var d = generacion.RequisitosGenerados.SingleOrDefault(x => x.PreregistroRequisitoId == Id(requisito.PreregistroRequisitoId))?.Definicion;
            if (d is null && generacion.EstadoCodigo == "GENERACION_PARCIAL")
                throw new ConflictoNegocioException("La determinación histórica del requisito requiere revisión de integridad.");
            if (d is null) return null; // Una historia sin snapshot no hereda la definición de la matriz actual.
            if (d.Codigo != requisito.RequisitoCatalogo.Codigo) throw new ConflictoNegocioException("La determinación histórica requiere revisión de integridad.");
            return new(generacion.MatrizVersion!, d.Nombre, d.TitularesCodigo, d.CantidadTitulares, d.CoberturaEsperada,
                true, d.TiposDocumentoCodigos, d.Fuentes);
        }

        private static void ValidarDatos(CrearVersionPreregistroDto datos)
        {
            Validator.ValidateObject(datos, new ValidationContext(datos), true);
            Validator.ValidateObject(datos.Encuesta, new ValidationContext(datos.Encuesta), true);
            if (datos.Contactos.Any(x => x is null)) throw new ValidationException("Cada contacto debe contener datos.");
            foreach (var c in datos.Contactos)
            {
                Validator.ValidateObject(c, new ValidationContext(c), true);
                if (Texto(c.Correo) is { } correo && !new EmailAddressAttribute().IsValid(correo))
                    throw new ValidationException("El correo declarado tiene un formato inválido.");
                if (Texto(c.Celular) is { } celular && !new PhoneAttribute().IsValid(celular))
                    throw new ValidationException("El celular declarado tiene un formato inválido.");
            }
            if (datos.Contactos.Count != 2 || datos.Contactos.Select(x => x.PosicionCodigo).Distinct().Count() != 2)
                throw new ValidationException("Debe indicar los contactos de A y B, incluso si sus valores están pendientes.");
            var e = datos.Encuesta;
            if (e.TieneHijos == false && (e.CantidadHijosMenores != 0 || e.CantidadHijosMayores != 0))
                throw new ValidationException("Sin hijos, ambas cantidades deben ser cero.");
            if (e.TieneHijosMayoresSituacionEspecial == true && (e.TieneHijos != true || e.CantidadHijosMayores <= 0))
                throw new ValidationException("La situación especial requiere declarar hijos y al menos un hijo mayor.");
            if (e.TieneBienes == false && e.TieneAcuerdoBienes == true)
                throw new ValidationException("Sin bienes no puede declarar un acuerdo de bienes.");
        }

        private static IEnumerable<(string Tipo, string? Valor)> Valores(ContactoPreregistroDto c)
            => [("CELULAR", Texto(c.Celular)), ("CORREO", Texto(c.Correo)), ("DIRECCION", Texto(c.Direccion))];
        private static RecursoNoEncontradoNegocioException NoEncontrado() => new("Prerregistro o versión no encontrados.");
        private static string Id(long id) => id.ToString(CultureInfo.InvariantCulture);
        private static string? Texto(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        private static string? Recortar(string? valor, int maximo)
            => Texto(valor) is { } texto ? texto[..Math.Min(texto.Length, maximo)] : null;
    }
}
