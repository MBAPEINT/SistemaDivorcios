using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Divorcios.Datos.Interfaces;
using Divorcios.Datos.Opciones;
using Divorcios.Dominio.Entidades;
using Divorcios.Negocio.DTOs.Identidad;
using Divorcios.Negocio.Excepciones;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Resultados;
using Microsoft.Extensions.Options;

namespace Divorcios.Negocio.Servicios
{
    public sealed class IdentidadServicio(IIdentidadRepositorio repositorio, IReniecProveedor proveedor,
        IOptions<ReniecOpciones> opciones, TimeProvider reloj) : IIdentidadServicio
    {
        public async Task<ResultadoConsultaIdentidad> ConsultarAsync(ConsultarDniDto datos, CancellationToken cancellationToken)
        {
            Validator.ValidateObject(datos, new ValidationContext(datos), true);
            // Primero caché: sigue siendo utilizable aunque la integración externa esté deshabilitada.
            var cache = await repositorio.ObtenerCacheAsync(datos.Dni, reloj.GetUtcNow().UtcDateTime, cancellationToken);
            if (cache is not null) return Mapear(cache, true);

            var configuracion = opciones.Value;
            if (!configuracion.EsValida)
                throw new ServicioNoDisponibleNegocioException("RENIEC_NO_CONFIGURADO", "La consulta externa de identidad todavía no está habilitada o configurada.");

            var resultado = await repositorio.EjecutarSerializadoAsync(datos.Dni, async () =>
            {
                var ahora = reloj.GetUtcNow().UtcDateTime;
                var existente = await repositorio.ObtenerCacheAsync(datos.Dni, ahora, cancellationToken);
                if (existente is not null) return (Consulta: existente, DesdeCache: true, Limite: false);
                // Hasta confirmar el reinicio del proveedor, no habilitar otro cupo por cambiar de fecha.
                if (await repositorio.ContarIntentosAsync(ahora.AddHours(-24), ahora, cancellationToken) >= configuracion.LimiteDiario!.Value)
                    return (Consulta: (ConsultaReniec?)null, DesdeCache: false, Limite: true);

                var consulta = new ConsultaReniec
                {
                    DniConsultado = datos.Dni, OrigenCodigo = "API", ConsultadoEn = ahora,
                    ResultadoCodigo = "ERROR"
                };
                await repositorio.GuardarAsync(consulta, cancellationToken);
                Divorcios.Datos.Resultados.RespuestaReniec respuesta;
                try { respuesta = await proveedor.ConsultarAsync(datos.Dni, cancellationToken); }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return (Consulta: (ConsultaReniec?)consulta, DesdeCache: false, Limite: false);
                }
                consulta.ResultadoCodigo = respuesta.Encontrado ? "ENCONTRADO" : "ERROR";
                consulta.Prenombres = respuesta.Prenombres;
                consulta.ApellidoPaterno = respuesta.ApellidoPaterno;
                consulta.ApellidoMaterno = respuesta.ApellidoMaterno;
                consulta.Direccion = respuesta.Direccion;
                consulta.CodigoHttp = respuesta.CodigoHttp;
                consulta.RespuestaHash = respuesta.RespuestaHash;
                // Sin caducidad configurada, conservar y reutilizar el resultado verificado en BD.
                consulta.ExpiraEn = respuesta.Encontrado && configuracion.VigenciaCacheMinutos is > 0
                    ? ahora.AddMinutes(configuracion.VigenciaCacheMinutos.Value) : null;
                await repositorio.GuardarAsync(consulta, CancellationToken.None);
                return (Consulta: (ConsultaReniec?)consulta, DesdeCache: false, Limite: false);
            }, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            if (resultado.Limite) throw new LimiteConsultasNegocioException();
            // Lanzar después del commit conserva el intento fallido y lo cuenta para la cuota.
            if (resultado.Consulta!.ResultadoCodigo == "ERROR")
                throw new ServicioNoDisponibleNegocioException("RENIEC_RESPUESTA_NO_VERIFICABLE", "No se pudo verificar la identidad con el proveedor. Intente más tarde.");
            return Mapear(resultado.Consulta, resultado.DesdeCache);
        }

        private static ResultadoConsultaIdentidad Mapear(ConsultaReniec consulta, bool desdeCache)
            => new(new(consulta.ConsultaReniecId.ToString(CultureInfo.InvariantCulture), consulta.ResultadoCodigo,
                consulta.PersonaId?.ToString(CultureInfo.InvariantCulture), consulta.Prenombres,
                consulta.ApellidoPaterno, consulta.ApellidoMaterno, consulta.ConsultadoEn, desdeCache), consulta.Direccion);
    }
}
