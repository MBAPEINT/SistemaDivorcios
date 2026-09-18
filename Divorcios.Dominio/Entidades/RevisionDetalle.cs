using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class RevisionDetalle
    {
        public long RevisionDetalleId { get; set; }
        public long RevisionPreregistroId { get; set; }
        public long PreregistroRequisitoId { get; set; }
        public string ResultadoCodigo { get; set; } = null!;
        public string? Observacion { get; set; }
        public RevisionPreregistro RevisionPreregistro { get; set; }
            = null!;
        public PreregistroRequisito PreregistroRequisito { get; set; }
            = null!;
    }
}