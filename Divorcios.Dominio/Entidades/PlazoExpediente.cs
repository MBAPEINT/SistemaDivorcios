using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class PlazoExpediente
    {
        public long PlazoExpedienteId { get; set; }
        public long ExpedienteId { get; set; }
        public short ReglaPlazoId { get; set; }
        public long? HistorialEstadoExpedienteOrigenId { get; set; }
        public int NumeroAplicacion { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public string EstadoCodigo { get; set; } = "PENDIENTE";
        public DateTime? CerradoEn { get; set; }
        public long? CreadoPorUsuarioId { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public string? Observacion { get; set; }
        public Expediente Expediente { get; set; } = null!;
        public ReglaPlazo ReglaPlazo { get; set; } = null!;
        public HistorialEstadoExpediente? HistorialEstadoExpedienteOrigen { get; set; }
        public UsuarioInterno? CreadoPorUsuario { get; set; }
    }
}