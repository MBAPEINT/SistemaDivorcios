using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class AudienciaRatificacion
    {
        public long AudienciaRatificacionId { get; set; }
        public long CasoId { get; set; }
        public short NumeroProgramacion { get; set; }
        public DateTime FechaHoraProgramada { get; set; }
        public string EstadoCodigo { get; set; } = "PROGRAMADA";
        public DateTime? FechaHoraRealizacion { get; set; }
        public DateTime? CerradoEn { get; set; }
        public long CreadaPorUsuarioId { get; set; }
        public DateTime CreadaEn { get; set; } = DateTime.UtcNow;
        public string? Observacion { get; set; }
        public Caso Caso { get; set; } = null!;
        public UsuarioInterno CreadaPorUsuario { get; set; } = null!;
        public ICollection<AsistenciaAudiencia> Asistencias { get; set; }
            = new List<AsistenciaAudiencia>();
    }
}