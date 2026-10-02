namespace Divorcios.Negocio.Resultados
{
    // La API mantiene la propiedad del stream de entrada y lo dispone al terminar la operación.
    public sealed record ArchivoEntrada(Stream Contenido, string NombreArchivo, long LongitudDeclarada);
    // El ejecutor HTTP toma la propiedad del stream de salida.
    public sealed record DescargaArchivo(Stream Contenido, string NombreArchivo, string MimeType);
}
