namespace Divorcios.Dominio.Entidades
{
    // Fija el archivo presentado para un requisito de una versión de encuesta.
    // Otra encuesta puede reutilizarlo creando otro vínculo, sin mover el anterior.
    public class PreregistroRequisitoDocumento
    {
        public long PreregistroRequisitoId { get; set; }
        public long DocumentoVersionId { get; set; }
        public PreregistroRequisito PreregistroRequisito { get; set; } = null!;
        public DocumentoVersion DocumentoVersion { get; set; } = null!;
    }
}
