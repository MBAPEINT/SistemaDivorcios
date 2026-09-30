using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class SolicitanteDisolucion
    {
        public long SolicitanteDisolucionId { get; set; }
        public long SolicitudDisolucionId { get; set; }
        public long ExpedienteConyugeId { get; set; }
        public string ModalidadCodigo { get; set; } = "DIRECTA";
        public long? RepresentacionId { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public SolicitudDisolucion SolicitudDisolucion { get; set; }
            = null!;
        public ExpedienteConyuge ExpedienteConyuge { get; set; } = null!;
        public Representacion? Representacion { get; set; }
    }
}
