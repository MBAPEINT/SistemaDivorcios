using System.ComponentModel.DataAnnotations;
using Divorcios.Negocio.Validaciones;

namespace Divorcios.Api.Modelos
{
    // Adaptador HTTP: IFormFile no sale de API; Negocio recibe ArchivoEntrada con Stream.
    public sealed class CargarArchivoPreregistroFormulario
    {
        [Required] public IFormFile Archivo { get; set; } = null!;
        [Required, Range(1, 32767)] public short? TipoDocumentoId { get; set; }
        [Required, MaxLength(200)] public string Titulo { get; set; } = null!;
    }
    public sealed class CorregirArchivoPreregistroFormulario
    {
        [Required] public IFormFile Archivo { get; set; } = null!;
        [Required, IdentificadorPositivo] public string DocumentoVersionBaseId { get; set; } = null!;
        [Required, MaxLength(300)] public string MotivoCambio { get; set; } = null!;
    }
}
