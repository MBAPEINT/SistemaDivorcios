using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Divorcios.Datos.Interfaces;
using Divorcios.Dominio.Entidades;
using Divorcios.Negocio.DTOs.Identidad;
using Divorcios.Negocio.Excepciones;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Opciones;
using Microsoft.Extensions.Options;

namespace Divorcios.Negocio.Servicios
{
    public sealed class AccesoCiudadanoServicio(IAccesoCiudadanoRepositorio repositorio,
        IIdentidadServicio identidad, IOptions<AccesoCiudadanoOpciones> opciones, TimeProvider reloj) : IAccesoCiudadanoServicio
    {
        public async Task<CiudadanoAutenticadoDto> IniciarSesionAsync(IniciarSesionCiudadanaDto datos, CancellationToken cancellationToken)
        {
            Validator.ValidateObject(datos, new ValidationContext(datos), true);
            var configuracion = opciones.Value;
            if (!configuracion.HabilitarDniDireccion || configuracion.MaximoIntentosFallidos <= 0
                || configuracion.BloqueoMinutos is <= 0 or > 1440)
                throw new ServicioNoDisponibleNegocioException("ACCESO_NO_CONFIGURADO", "El acceso ciudadano no está habilitado.");

            var previa = await repositorio.ObtenerPorDniAsync(datos.Dni, cancellationToken);
            if (previa is not null && !Disponible(previa)) throw new AccesoCiudadanoRechazadoException();

            var consulta = await identidad.ConsultarAsync(new() { Dni = datos.Dni }, cancellationToken);
            var direccion = Normalizar(consulta.Direccion);
            var recibido = Normalizar(datos.SufijoDireccion);
            var correcto = direccion.Length >= 3 && recibido.Length == 3
                && CryptographicOperations.FixedTimeEquals(
                    SHA256.HashData(Encoding.UTF8.GetBytes(direccion[^3..])),
                    SHA256.HashData(Encoding.UTF8.GetBytes(recibido)));

            var resultado = await repositorio.EjecutarSerializadoAsync(async () =>
            {
                var ahora = reloj.GetUtcNow().UtcDateTime;
                var cuenta = await repositorio.ObtenerPorDniAsync(datos.Dni, cancellationToken);
                if (cuenta is not null && !Disponible(cuenta)) return null;
                if (!correcto || consulta.Datos.PersonaId is null)
                {
                    // No crear una cuenta por un sufijo incorrecto. La consulta RENIEC conserva su resultado propio.
                    if (cuenta is not null)
                    {
                        if (cuenta.BloqueadoHasta <= ahora) cuenta.IntentosFallidos = 0;
                        cuenta.IntentosFallidos = (short)Math.Min(cuenta.IntentosFallidos + 1, configuracion.MaximoIntentosFallidos);
                        if (cuenta.IntentosFallidos >= configuracion.MaximoIntentosFallidos)
                            cuenta.BloqueadoHasta = ahora.AddMinutes(configuracion.BloqueoMinutos);
                        await repositorio.GuardarAsync(cuenta, cancellationToken);
                    }
                    return null;
                }
                cuenta ??= new CuentaCiudadana
                {
                    PersonaId = long.Parse(consulta.Datos.PersonaId, CultureInfo.InvariantCulture),
                    EstadoCodigo = "ACTIVA", CreadoEn = ahora
                };
                cuenta.IntentosFallidos = 0;
                cuenta.BloqueadoHasta = null;
                cuenta.UltimoAccesoEn = ahora;
                await repositorio.GuardarAsync(cuenta, cancellationToken);
                return new CiudadanoAutenticadoDto(cuenta.CuentaCiudadanaId.ToString(CultureInfo.InvariantCulture),
                    consulta.Datos.PersonaId, consulta.Datos.Nombres!, consulta.Datos.ApellidoPaterno!, consulta.Datos.ApellidoMaterno!);
            }, cancellationToken);
            // El commit anterior conserva los intentos fallidos aunque el HTTP termine en 401.
            return resultado ?? throw new AccesoCiudadanoRechazadoException();
        }

        public async Task<CiudadanoAutenticadoDto?> ObtenerSesionAsync(long cuentaId, CancellationToken cancellationToken)
        {
            if (!opciones.Value.HabilitarDniDireccion) return null;
            var cuenta = await repositorio.ObtenerPorIdAsync(cuentaId, cancellationToken);
            if (cuenta is null || !Disponible(cuenta)) return null;
            return new(cuenta.CuentaCiudadanaId.ToString(CultureInfo.InvariantCulture),
                cuenta.PersonaId.ToString(CultureInfo.InvariantCulture), cuenta.Persona.Nombres,
                cuenta.Persona.ApellidoPaterno, cuenta.Persona.ApellidoMaterno);
        }

        private bool Disponible(CuentaCiudadana cuenta) => cuenta.EstadoCodigo == "ACTIVA"
            && !(cuenta.BloqueadoHasta > reloj.GetUtcNow().UtcDateTime);
        private static string Normalizar(string? texto) => (texto ?? "").Trim().Normalize(NormalizationForm.FormC).ToUpperInvariant();
    }
}
