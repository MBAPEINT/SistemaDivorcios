using System.ComponentModel.DataAnnotations;

namespace Divorcios.Negocio.DTOs.Identidad
{
    public sealed class ConsultarDniDto
    {
        [Required, RegularExpression("^[0-9]{8}$", ErrorMessage = "El DNI debe tener ocho dígitos.")]
        public string Dni { get; set; } = null!;
    }
}
