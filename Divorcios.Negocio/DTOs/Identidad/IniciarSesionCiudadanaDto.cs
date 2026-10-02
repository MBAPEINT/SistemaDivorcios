using System.ComponentModel.DataAnnotations;

namespace Divorcios.Negocio.DTOs.Identidad
{
    public sealed class IniciarSesionCiudadanaDto
    {
        [Required, RegularExpression("^[0-9]{8}$", ErrorMessage = "El DNI debe tener ocho dígitos.")]
        public string Dni { get; set; } = null!;

        [Required, StringLength(3, MinimumLength = 3, ErrorMessage = "Ingrese los últimos tres caracteres de la dirección.")]
        public string SufijoDireccion { get; set; } = null!;
    }
    public sealed record CiudadanoAutenticadoDto(string CuentaCiudadanaId, string PersonaId, string Nombres,
        string ApellidoPaterno, string ApellidoMaterno);
}
