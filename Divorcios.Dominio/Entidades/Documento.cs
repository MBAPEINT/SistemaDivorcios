using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class Documento
    {
        public long DocumentoId { get; set; }
        public long CasoId { get; set; }
        public short TipoDocumentoId { get; set; }
        public long? PreregistroRequisitoId { get; set; }
        public long? ActuacionAdministrativaId { get; set; }
        public string Titulo { get; set; } = null!;
        public string EtapaCodigo { get; set; } = null!;
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public Caso Caso { get; set; } = null!;
        public TipoDocumento TipoDocumento { get; set; } = null!;
        public PreregistroRequisito? PreregistroRequisito { get; set; }
        public Representacion? RepresentacionPoder { get; set; }
        public SolicitudDisolucion? SolicitudDisolucion { get; set; }
        public ActuacionAdministrativa? ActuacionAdministrativa { get; set; }
        public PagoTramite? PagoTramite { get; set; }
        public Oficio? Oficio { get; set; }
        public ICollection<DocumentoVersion> Versiones { get; set; }
            = new List<DocumentoVersion>();
    }
}