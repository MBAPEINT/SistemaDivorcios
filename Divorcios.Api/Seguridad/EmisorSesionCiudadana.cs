using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Divorcios.Negocio.DTOs.Identidad;
using Divorcios.Negocio.Excepciones;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Divorcios.Api.Seguridad
{
    public sealed record SesionCiudadanaDto(string AccessToken, string TokenType, DateTime ExpiraEn,
        CiudadanoAutenticadoDto Ciudadano, string MetodoAcceso);

    public sealed class EmisorSesionCiudadana(IOptions<JwtCiudadanoOpciones> opciones, TimeProvider reloj)
    {
        public void ComprobarConfiguracion()
        {
            if (!opciones.Value.EsValida)
                throw new ServicioNoDisponibleNegocioException("SESION_NO_CONFIGURADA", "La emisión de sesiones todavía no está configurada.");
        }

        public SesionCiudadanaDto Emitir(CiudadanoAutenticadoDto ciudadano)
        {
            ComprobarConfiguracion();
            var configuracion = opciones.Value;
            var ahora = reloj.GetUtcNow().UtcDateTime;
            var expira = ahora.AddMinutes(configuracion.DuracionMinutos);
            Claim[] claims =
            [
                new(JwtRegisteredClaimNames.Sub, ciudadano.CuentaCiudadanaId),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new("persona_id", ciudadano.PersonaId),
                new("metodo_acceso", "dni_direccion"),
                new("permiso", "identidad.consultar-dni")
            ];
            var token = new JwtSecurityToken(configuracion.Emisor, configuracion.Audiencia, claims,
                ahora, expira, new SigningCredentials(new SymmetricSecurityKey(configuracion.ObtenerClave()!), SecurityAlgorithms.HmacSha256));
            return new(new JwtSecurityTokenHandler().WriteToken(token), "Bearer", expira, ciudadano, "dni_direccion");
        }
    }
}
