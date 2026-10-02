namespace Divorcios.Negocio.Opciones
{
    public sealed class AccesoCiudadanoOpciones
    {
        public const string Seccion = "AccesoCiudadano";
        public bool HabilitarDniDireccion { get; set; }
        // Límites técnicos del acceso provisional, configurables; no son plazos del trámite.
        public short MaximoIntentosFallidos { get; set; } = 5;
        public int BloqueoMinutos { get; set; } = 15;
    }
}
