using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class Representacion
    {
        public long RepresentacionId { get; set; }
        public long CasoConyugeId { get; set; }
        public long RepresentantePersonaId { get; set; }
        public long DocumentoPoderId { get; set; }
        public DateOnly VigenteDesde { get; set; }
        public DateOnly? VigenteHasta { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public CasoConyuge CasoConyuge { get; set; } = null!;
        public Persona RepresentantePersona { get; set; } = null!;
        public Documento DocumentoPoder { get; set; } = null!;
        public ICollection<AsistenciaAudiencia> AsistenciasAudiencia { get; set; }
            = new List<AsistenciaAudiencia>();
        public ICollection<SolicitanteDisolucion> SolicitudesDisolucionPresentadas { get; set; }
            = new List<SolicitanteDisolucion>();
    }
}