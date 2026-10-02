namespace Divorcios.Datos.Excepciones
{
    public sealed class ArchivoPersistenciaException(string codigo, Exception? causa = null)
        : Exception("No se pudo procesar el archivo privado.", causa)
    {
        public string Codigo { get; } = codigo;
    }
}
