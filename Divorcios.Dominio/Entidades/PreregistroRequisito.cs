using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class PreregistroRequisito
    {
        public long PreregistroRequisitoId { get; set; }
        public long PreregistroId { get; set; }
        public short RequisitoCatalogoId { get; set; }
        public bool Obligatorio { get; set; } = true;
        public string EstadoCodigo { get; set; } = "PENDIENTE";
        public Preregistro Preregistro { get; set; } = null!;
        public RequisitoCatalogo RequisitoCatalogo { get; set; } = null!;
        public ICollection<Documento> Documentos { get; set; }
            = new List<Documento>();
        public ICollection<RevisionDetalle> DetallesRevision { get; set; }
            = new List<RevisionDetalle>();
    }
}