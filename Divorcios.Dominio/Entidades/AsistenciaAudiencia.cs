using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class AsistenciaAudiencia
    {
        public long AsistenciaAudienciaId { get; set; }
        public long AudienciaRatificacionId { get; set; }
        public long ExpedienteConyugeId { get; set; }
        public string ModalidadCodigo { get; set; } = "DIRECTA";
        public long? RepresentacionId { get; set; }
        public bool Asistio { get; set; }
        // null: sin respuesta registrada o no corresponde, incluyendo inasistencia.
        // Tanto false (negativa expresa) como true requieren asistencia e identidad verificada.
        public bool? RatificoVoluntad { get; set; }
        public DateTime? IdentidadVerificadaEn { get; set; }
        public string? Observacion { get; set; }
        public AudienciaRatificacion AudienciaRatificacion { get; set; }
            = null!;
        public ExpedienteConyuge ExpedienteConyuge { get; set; } = null!;
        public Representacion? Representacion { get; set; }
    }
}
