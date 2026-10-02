using Divorcios.Negocio.DTOs.Preregistro;

namespace Divorcios.Negocio.Opciones
{
    public sealed class InformacionPreregistroOpciones
    {
        public const string Seccion = "Preregistro:Informacion";
        public List<FormatoPreregistroDto> Formatos { get; set; } = [];
        public List<string> MimePermitidos { get; set; } = [];
        public long? MaximoBytes { get; set; }
    }
}
