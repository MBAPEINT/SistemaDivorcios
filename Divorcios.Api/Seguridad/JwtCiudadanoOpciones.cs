namespace Divorcios.Api.Seguridad
{
    public sealed class JwtCiudadanoOpciones
    {
        public const string Seccion = "JwtCiudadano";
        public string Emisor { get; set; } = "SistemaDivorcios";
        public string Audiencia { get; set; } = "SistemaDivorcios.Ciudadanos";
        public string? ClaveBase64 { get; set; }
        public int DuracionMinutos { get; set; } = 15;

        public byte[]? ObtenerClave()
        {
            try
            {
                var clave = Convert.FromBase64String(ClaveBase64 ?? "");
                return clave.Length >= 32 ? clave : null;
            }
            catch (FormatException) { return null; }
        }
        public bool EsValida => ObtenerClave() is not null && !string.IsNullOrWhiteSpace(Emisor)
            && !string.IsNullOrWhiteSpace(Audiencia) && DuracionMinutos is > 0 and <= 60;
    }
}
