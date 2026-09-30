using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class ExpedienteConyuge
    {
        public long ExpedienteConyugeId { get; set; }
        public long ExpedienteId { get; set; }
        public long PersonaId { get; set; }
        public string PosicionCodigo { get; set; } = null!;
        public Expediente Expediente { get; set; } = null!;
        public Persona Persona { get; set; } = null!;
        public bool EsIniciador { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public ICollection<Representacion> Representaciones { get; set; }
            = new List<Representacion>();
        public ICollection<AsistenciaAudiencia> AsistenciasAudiencia { get; set; }
            = new List<AsistenciaAudiencia>();
        public ICollection<SolicitanteDisolucion> SolicitudesDisolucionPresentadas { get; set; }
            = new List<SolicitanteDisolucion>();
        public ICollection<ExpedienteContactoHistorial> ContactosHistorial { get; set; } 
            = new List<ExpedienteContactoHistorial>();
        public ICollection<Pago> PagosRealizados { get; set; }
            = new List<Pago>();
    }
}
