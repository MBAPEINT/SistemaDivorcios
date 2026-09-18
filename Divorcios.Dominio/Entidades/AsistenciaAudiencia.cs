using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class AsistenciaAudiencia
    {
        public long AsistenciaAudienciaId { get; set; }
        public long AudienciaRatificacionId { get; set; }
        public long CasoConyugeId { get; set; }
        public string ModalidadCodigo { get; set; } = "DIRECTA";
        public long? RepresentacionId { get; set; }
        public bool Asistio { get; set; }
        public bool RatificoVoluntad { get; set; }
        public DateTime? IdentidadVerificadaEn { get; set; }
        public string? Observacion { get; set; }
        public AudienciaRatificacion AudienciaRatificacion { get; set; }
            = null!;
        public CasoConyuge CasoConyuge { get; set; } = null!;
        public Representacion? Representacion { get; set; }
    }
}