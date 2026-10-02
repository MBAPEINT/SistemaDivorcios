using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Divorcios.Datos.Excepciones;
using Divorcios.Datos.Interfaces;
using Divorcios.Datos.Resultados;
using Divorcios.Dominio.Entidades;
using Divorcios.Negocio.DTOs.Comun;
using Divorcios.Negocio.DTOs.Preregistro;
using Divorcios.Negocio.Excepciones;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Resultados;
using Divorcios.Negocio.Validaciones;

namespace Divorcios.Negocio.Servicios
{
    public sealed class PreregistrosServicio(IPreregistrosRepositorio repositorio, TimeProvider reloj) : IPreregistrosServicio
    {
        public async Task<PreregistroDetalleDto> CrearAsync(ActorCiudadano actor, CrearPreregistroDto datos, CancellationToken cancellationToken)
        {
            Validator.ValidateObject(datos, new ValidationContext(datos), true);
            if (datos.Conyuges.Any(x => x is null)) throw new ValidationException("Cada cónyuge debe contener datos.");
            foreach (var conyuge in datos.Conyuges) Validator.ValidateObject(conyuge, new ValidationContext(conyuge), true);
            if (datos.Conyuges.Count != 2 || datos.Conyuges.Select(x => x.PosicionCodigo).Distinct().Count() != 2)
                throw new ValidationException("Debe registrar exactamente un cónyuge A y un cónyuge B.");
            var participantes = datos.Conyuges.Select(x => new
            {
                PersonaId = IdentificadorPositivoAttribute.Leer(x.PersonaId), x.PosicionCodigo, EsIniciador = x.EsIniciador!.Value
            }).ToArray();
            if (participantes.Select(x => x.PersonaId).Distinct().Count() != 2)
                throw new ValidationException("Los cónyuges deben ser personas diferentes.");
            if (participantes.Count(x => x.EsIniciador) != 1)
                throw new ValidationException("Debe existir exactamente un iniciador.");
            if (participantes.Single(x => x.EsIniciador).PersonaId != actor.PersonaId)
                throw new ValidationException("El iniciador debe corresponder a la persona de la sesión.");

            try
            {
                return await repositorio.EjecutarTransaccionAsync(async () =>
                {
                    await ValidarActorAsync(actor, cancellationToken);
                    var personas = await repositorio.ObtenerPersonasAsync(participantes.Select(x => x.PersonaId).ToArray(), cancellationToken);
                    if (personas.Count != 2 || personas.Any(x => !x.VerificadoReniec || x.VerificadoReniecEn is null))
                        throw new ValidationException("Ambas personas deben existir y tener datos de identidad verificados antes de crear el prerregistro.");

                    var ahora = reloj.GetUtcNow().UtcDateTime;
                    var expediente = new Expediente
                    {
                        CodigoPreregistro = "PRE-" + Convert.ToBase64String(RandomNumberGenerator.GetBytes(16))
                            .TrimEnd('=').Replace('+', '-').Replace('/', '_'),
                        CreadoPorCuentaId = actor.CuentaCiudadanaId, FechaInicioDigital = ahora, CreadoEn = ahora,
                        Conyuges = participantes.Select(x => new ExpedienteConyuge
                        {
                            PersonaId = x.PersonaId, PosicionCodigo = x.PosicionCodigo, EsIniciador = x.EsIniciador, CreadoEn = ahora
                        }).ToList()
                    };
                    var nuevo = new Preregistro { Expediente = expediente, EstadoCodigo = "BORRADOR", CreadoEn = ahora };
                    await repositorio.AgregarAsync(nuevo, cancellationToken);
                    await repositorio.RegistrarAuditoriaAsync(new RegistroAuditoria
                    {
                        ActorTipoCodigo = "CUENTA_CIUDADANA", CuentaCiudadanaId = actor.CuentaCiudadanaId,
                        ExpedienteId = expediente.ExpedienteId, AccionCodigo = "PREREGISTRO_CREADO",
                        RecursoCodigo = "PREREGISTRO", RecursoId = Id(nuevo.PreregistroId), ResultadoCodigo = "EXITO",
                        Descripcion = "Creación del prerregistro digital con sus dos participantes y un iniciador.",
                        DetalleJson = JsonSerializer.Serialize(new { expediente.CodigoPreregistro,
                            Conyuges = participantes.Select(x => new { PersonaId = Id(x.PersonaId), x.PosicionCodigo, x.EsIniciador }) }),
                        CorrelacionId = actor.CorrelacionId == Guid.Empty ? Guid.NewGuid() : actor.CorrelacionId,
                        DireccionIp = Recortar(actor.DireccionIp, 64), UserAgent = Recortar(actor.UserAgent, 500), RegistradoEn = ahora
                    }, cancellationToken);
                    var cabecera = await repositorio.ObtenerAsync(nuevo.PreregistroId, actor.PersonaId, cancellationToken)
                        ?? throw new InvalidOperationException("No se pudo recuperar el prerregistro recién creado.");
                    return MapearDetalle(cabecera, actor.PersonaId);
                }, cancellationToken);
            }
            catch (CodigoDuplicadoPersistenciaException excepcion)
            {
                throw new ConflictoNegocioException("No se pudo reservar un código digital único. Vuelva a intentar la creación.", excepcion);
            }
        }

        public async Task<PaginaDto<PreregistroResumenDto>> ListarAsync(ActorCiudadano actor, ListarPreregistrosDto filtro, CancellationToken cancellationToken)
        {
            Validator.ValidateObject(filtro, new ValidationContext(filtro), true);
            if (filtro.EstadoCodigo is not null && filtro.EstadoCodigo.Length == 0)
                throw new ValidationException("El estado debe ser uno de los códigos admitidos.");
            await ValidarActorAsync(actor, cancellationToken);
            var pagina = await repositorio.ListarAsync(actor.PersonaId, filtro.EstadoCodigo, filtro.Pagina, filtro.TamanoPagina, cancellationToken);
            return new(pagina.Items.Select(x => new PreregistroResumenDto(Id(x.PreregistroId), Id(x.ExpedienteId),
                x.Expediente.CodigoPreregistro, x.Expediente.NumeroExpediente, x.EstadoCodigo, x.CreadoEn,
                x.EnviadoEn, EstaBloqueado(x), SoyIniciador(x, actor.PersonaId), MapearConyuges(x))).ToArray(),
                filtro.Pagina, filtro.TamanoPagina, pagina.Total);
        }

        public async Task<PreregistroDetalleDto> ObtenerAsync(ActorCiudadano actor, string preregistroId, CancellationToken cancellationToken)
        {
            var id = IdentificadorPositivoAttribute.Leer(preregistroId);
            await ValidarActorAsync(actor, cancellationToken);
            var cabecera = await repositorio.ObtenerAsync(id, actor.PersonaId, cancellationToken)
                ?? throw new RecursoNoEncontradoNegocioException("Prerregistro no encontrado.");
            return MapearDetalle(cabecera, actor.PersonaId);
        }

        private async Task ValidarActorAsync(ActorCiudadano actor, CancellationToken cancellationToken)
        {
            if (actor.CuentaCiudadanaId <= 0 || actor.PersonaId <= 0) throw new AccesoCiudadanoRechazadoException();
            var cuenta = await repositorio.ObtenerCuentaAsync(actor.CuentaCiudadanaId, cancellationToken);
            if (cuenta is null || cuenta.PersonaId != actor.PersonaId || cuenta.EstadoCodigo != "ACTIVA"
                || cuenta.BloqueadoHasta > reloj.GetUtcNow().UtcDateTime)
                throw new AccesoCiudadanoRechazadoException();
        }

        private static PreregistroDetalleDto MapearDetalle(CabeceraPreregistro cabecera, long personaId)
        {
            var registro = cabecera.Registro;
            var bloqueado = EstaBloqueado(registro);
            var iniciador = SoyIniciador(registro, personaId);
            var abiertas = cabecera.Revisiones.Where(x => x.ResultadoCodigo == "EN_REVISION" && x.FinalizadaEn is null).ToArray();
            var trabajoHabilitado = !bloqueado && (registro.EstadoCodigo is "BORRADOR" or "OBSERVADO") && abiertas.Length == 0;
            var pendientes = new List<string>();
            long? enviada = null;
            if (registro.EnviadoEn is not null)
            {
                if (long.TryParse(cabecera.VersionEnviadaAuditoriaId, NumberStyles.None, CultureInfo.InvariantCulture, out var auditada)
                    && cabecera.Versiones.Any(x => x.VersionId == auditada)) enviada = auditada;
                else
                {
                    // Una revisión acredita su versión evaluada. Nunca adivinar el envío usando la última encuesta.
                    var evaluadas = cabecera.Revisiones.Where(x => x.IniciadaEn >= registro.EnviadoEn)
                        .Select(x => x.VersionId).Distinct().ToArray();
                    if (evaluadas.Length == 1) enviada = evaluadas[0];
                }
                if (enviada is null) pendientes.Add("VERSION_ENVIADA");
            }
            long? aprobada = null;
            if (registro.AprobadoEn is not null)
            {
                var conformes = cabecera.Revisiones.Where(x => x.ResultadoCodigo == "APROBADO" && x.FinalizadaEn == registro.AprobadoEn)
                    .Select(x => x.VersionId).Distinct().ToArray();
                if (conformes.Length == 1) aprobada = conformes[0];
                else pendientes.Add("VERSION_APROBADA");
            }
            if (abiertas.Length > 1) pendientes.Add("REVISION_ABIERTA_AMBIGUA");
            return new(Id(registro.PreregistroId), Id(registro.ExpedienteId), registro.Expediente.CodigoPreregistro,
                registro.Expediente.NumeroExpediente, registro.EstadoCodigo, registro.CreadoEn, registro.EnviadoEn,
                registro.AprobadoEn, registro.BloqueadoEn ?? registro.Expediente.PreregistroBloqueadoEn,
                bloqueado, iniciador, MapearConyuges(registro),
                cabecera.Versiones.Count > 0 && ReglasEdicionPreregistro.EsVersionTrabajo(cabecera, cabecera.Versiones[0].VersionId)
                    ? Id(cabecera.Versiones[0].VersionId) : null,
                enviada.HasValue ? Id(enviada.Value) : null, aprobada.HasValue ? Id(aprobada.Value) : null,
                abiertas.Length == 1 ? Id(abiertas[0].RevisionId) : null,
                trabajoHabilitado && iniciador ? ["CONSULTAR", "CREAR_VERSION"] : ["CONSULTAR"], [], pendientes);
        }

        private static bool SoyIniciador(Preregistro registro, long personaId)
            => registro.Expediente.Conyuges.Any(x => x.PersonaId == personaId && x.EsIniciador);
        private static bool EstaBloqueado(Preregistro registro) => registro.BloqueadoEn is not null
            || registro.Expediente.PreregistroBloqueadoEn is not null || registro.Expediente.OficializadoEn is not null
            || registro.Expediente.NumeroExpediente is not null || registro.Expediente.CerradoEn is not null
            || registro.EstadoCodigo == "CANCELADO";
        private static IReadOnlyList<ConyugePreregistroDto> MapearConyuges(Preregistro registro)
            => registro.Expediente.Conyuges.OrderBy(x => x.PosicionCodigo).Select(x => new ConyugePreregistroDto(
                Id(x.ExpedienteConyugeId), Id(x.PersonaId), x.PosicionCodigo, x.EsIniciador,
                x.Persona.Nombres, x.Persona.ApellidoPaterno, x.Persona.ApellidoMaterno)).ToArray();
        private static string Id(long id) => id.ToString(CultureInfo.InvariantCulture);
        private static string? Recortar(string? valor, int maximo)
        {
            var normalizado = valor?.Trim();
            return string.IsNullOrEmpty(normalizado) ? null : normalizado[..Math.Min(normalizado.Length, maximo)];
        }
    }
}
