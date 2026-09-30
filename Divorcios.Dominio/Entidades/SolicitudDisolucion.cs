using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class SolicitudDisolucion
    {
        public long SolicitudDisolucionId { get; set; }
        public long ExpedienteId { get; set; }
        public long DocumentoSolicitudId { get; set; }
        public DateOnly FechaPresentacionMesaPartes { get; set; }
        public string EstadoCodigo { get; set; } = "PRESENTADA";
        public long RegistradaPorUsuarioId { get; set; }
        public DateTime RegistradaEn { get; set; } = DateTime.UtcNow;
        public long? ValidadaPorUsuarioId { get; set; }
        public DateTime? ValidadaEn { get; set; }
        public string? Observacion { get; set; }
        public Expediente Expediente { get; set; } = null!;
        public Documento DocumentoSolicitud { get; set; } = null!;
        public UsuarioInterno RegistradaPorUsuario { get; set; } = null!;
        public UsuarioInterno? ValidadaPorUsuario { get; set; }
        public ICollection<SolicitanteDisolucion> Solicitantes { get; set; }
            = new List<SolicitanteDisolucion>();
    }
}