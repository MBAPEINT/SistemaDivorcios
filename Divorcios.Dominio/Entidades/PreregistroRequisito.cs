using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class PreregistroRequisito
    {
        public long PreregistroRequisitoId { get; set; }
        public long PreregistroVersionId { get; set; }
        public short RequisitoCatalogoId { get; set; }
        public bool Obligatorio { get; set; } = true;
        public string EstadoCodigo { get; set; } = "PENDIENTE";
        public bool Aplica { get; set; } = true;
        public DateTime GeneradoEn { get; set; } = DateTime.UtcNow;
        public PreregistroVersion PreregistroVersion { get; set; } = null!;
        public RequisitoCatalogo RequisitoCatalogo { get; set; } = null!;
        // Documentos cuyo origen es este requisito, no todos los archivos reutilizados.
        public ICollection<Documento> Documentos { get; set; }
            = new List<Documento>();
        public ICollection<PreregistroRequisitoDocumento> ArchivosPresentados { get; set; }
            = new List<PreregistroRequisitoDocumento>();
        public ICollection<RevisionDetalle> DetallesRevision { get; set; }
            = new List<RevisionDetalle>();
    }
}
