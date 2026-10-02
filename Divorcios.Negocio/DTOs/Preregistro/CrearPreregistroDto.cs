using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Divorcios.Negocio.Validaciones;

namespace Divorcios.Negocio.DTOs.Preregistro
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed class CrearPreregistroDto
    {
        [Required, MinLength(2), MaxLength(2)]
        public List<CrearConyugeDto> Conyuges { get; set; } = null!;
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed class CrearConyugeDto
    {
        [Required, RegularExpression("^(A|B)$")]
        public string PosicionCodigo { get; set; } = null!;

        [Required, IdentificadorPositivo]
        public string PersonaId { get; set; } = null!;

        [Required]
        public bool? EsIniciador { get; set; }
    }
}
