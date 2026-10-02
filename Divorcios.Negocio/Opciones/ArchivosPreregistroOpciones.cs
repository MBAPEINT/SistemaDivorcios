namespace Divorcios.Negocio.Opciones
{
    public sealed class ArchivosPreregistroOpciones
    {
        public const string Seccion = "Preregistro:Archivos";
        // Sólo configuración del servidor validada por el responsable funcional, nunca el body ciudadano.
        public List<TiposRequisitoOpciones> TiposPorRequisito { get; set; } = [];
    }
    public sealed class TiposRequisitoOpciones
    {
        public string RequisitoCodigo { get; set; } = "";
        public bool Confirmada { get; set; }
        public List<string> TiposDocumentoCodigos { get; set; } = [];
    }
}
