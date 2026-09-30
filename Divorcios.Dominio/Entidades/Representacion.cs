using System;
using System.Collections.Generic;

namespace Divorcios.Dominio.Entidades
{
    public class Representacion
    {
        public long RepresentacionId { get; set; }
        public long ExpedienteConyugeId { get; set; }
        public long RepresentantePersonaId { get; set; }
        public long? DocumentoPoderId { get; set; }
        public string TipoPoderCodigo { get; set; } = "ESPECIAL";
        public string EstadoCodigo { get; set; } = "VIGENTE";
        public DateOnly VigenteDesde { get; set; }
        public DateOnly? VigenteHasta { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public ExpedienteConyuge ExpedienteConyuge { get; set; } = null!;
        public Persona RepresentantePersona { get; set; } = null!;
        public Documento? DocumentoPoder { get; set; }
        public ICollection<AsistenciaAudiencia> AsistenciasAudiencia { get; set; } 
            = new List<AsistenciaAudiencia>();
        public ICollection<SolicitanteDisolucion> SolicitudesDisolucionPresentadas { get; set; }
            = new List<SolicitanteDisolucion>();
    }
}