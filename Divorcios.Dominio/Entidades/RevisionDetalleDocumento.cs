namespace Divorcios.Dominio.Entidades
{
    // Conserva la versión exacta de cada archivo evaluado para un requisito.
    public class RevisionDetalleDocumento
    {
        public long RevisionDetalleId { get; set; }
        public long DocumentoVersionId { get; set; }
        public RevisionDetalle RevisionDetalle { get; set; } = null!;
        public DocumentoVersion DocumentoVersion { get; set; } = null!;
    }
}
