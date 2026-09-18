using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class RevisionPreregistro
    {
        public long RevisionPreregistroId { get; set; }
        public long PreregistroId { get; set; }
        public short NumeroRevision { get; set; }
        public long RevisadoPorUsuarioId { get; set; }
        public string ResultadoCodigo { get; set; } = "EN_REVISION";
        public string? ComentarioGeneral { get; set; }
        public DateTime IniciadaEn { get; set; } = DateTime.UtcNow;
        public DateTime? FinalizadaEn { get; set; }
        public Preregistro Preregistro { get; set; } = null!;
        public UsuarioInterno RevisadoPorUsuario { get; set; } = null!;
        public ICollection<RevisionDetalle> Detalles { get; set; }
            = new List<RevisionDetalle>();
    }
}