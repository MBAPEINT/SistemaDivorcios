using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class SolicitanteDisolucion
    {
        public long SolicitanteDisolucionId { get; set; }
        public long SolicitudDisolucionId { get; set; }
        public long CasoConyugeId { get; set; }
        public string ModalidadCodigo { get; set; } = "DIRECTA";
        public long? RepresentacionId { get; set; }
        public SolicitudDisolucion SolicitudDisolucion { get; set; }
            = null!;
        public CasoConyuge CasoConyuge { get; set; } = null!;
        public Representacion? Representacion { get; set; }
    }
}