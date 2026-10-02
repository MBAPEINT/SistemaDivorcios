using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Divorcios.Negocio.Validaciones;

namespace Divorcios.Negocio.DTOs.Preregistro
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed class CrearVersionPreregistroDto
    {
        [IdentificadorPositivo(PermitirNull = true)] public string? VersionBaseId { get; set; }
        [MaxLength(300)] public string? MotivoCambio { get; set; }
        [Required] public EncuestaPreregistroEntradaDto Encuesta { get; set; } = null!;
        [Required, MinLength(2), MaxLength(2)]
        public List<ContactoPreregistroDto> Contactos { get; set; } = null!;
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed class EncuestaPreregistroEntradaDto
    {
        [Required] public DateOnly? FechaMatrimonio { get; set; }
        [Required] public bool? MatrimonioEnPorvenir { get; set; }
        [Required] public bool? UltimoDomicilioConyugalPorvenir { get; set; }
        [MaxLength(250)] public string? DomicilioConyugal { get; set; }
        [Required] public bool? TieneHijos { get; set; }
        [Required, Range(0, 32767)] public int? CantidadHijosMenores { get; set; }
        [Required, Range(0, 32767)] public int? CantidadHijosMayores { get; set; }
        public bool? TieneHijosMayoresSituacionEspecial { get; set; }
        [Required] public bool? TieneBienes { get; set; }
        [Required] public bool? TieneAcuerdoBienes { get; set; }
        public bool? RequiereRepresentacionA { get; set; }
        public bool? RequiereRepresentacionB { get; set; }
        [MaxLength(2000)] public string? ObservacionCiudadano { get; set; }
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed class ContactoPreregistroDto
    {
        [Required, RegularExpression("^(A|B)$")]
        public string PosicionCodigo { get; set; } = null!;
        [MaxLength(250)] public string? Celular { get; set; }
        [MaxLength(250)] public string? Correo { get; set; }
        [MaxLength(250)] public string? Direccion { get; set; }
    }
}
