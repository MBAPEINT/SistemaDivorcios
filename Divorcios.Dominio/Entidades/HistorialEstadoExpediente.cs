using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class HistorialEstadoExpediente
    {
        public long HistorialEstadoExpedienteId { get; set; }
        public long ExpedienteId { get; set; }
        public short EstadoExpedienteId { get; set; }
        public int NumeroSecuencia { get; set; }
        public long? RegistradoPorUsuarioId { get; set; }
        public DateTime IniciadoEn { get; set; }
        public DateTime? FinalizadoEn { get; set; }
        public DateTime RegistradoEn { get; set; } = DateTime.UtcNow;
        // Conserva la observación o justificación del cambio de estado.
        public string? Observacion { get; set; }
        public Expediente Expediente { get; set; } = null!;
        public EstadoExpediente EstadoExpediente { get; set; } = null!;
        public UsuarioInterno? RegistradoPorUsuario { get; set; }
        public ICollection<PlazoExpediente> PlazosOriginados { get; set; }
            = new List<PlazoExpediente>();
    }
}
