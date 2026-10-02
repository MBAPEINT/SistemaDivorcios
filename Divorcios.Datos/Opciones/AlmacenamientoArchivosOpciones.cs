namespace Divorcios.Datos.Opciones
{
    public sealed class AlmacenamientoArchivosOpciones
    {
        public const string Seccion = "AlmacenamientoArchivos";
        // Por defecto, carpeta privada del usuario del proceso, independiente de bin y del repositorio.
        public string? DirectorioRaiz { get; set; }
    }
}
