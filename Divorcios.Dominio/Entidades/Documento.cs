using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class Documento
    {
        public long DocumentoId { get; set; }
        public long ExpedienteId { get; set; }
        public short TipoDocumentoId { get; set; }
        // Requisito de origen del documento. La reutilización no cambia esta FK;
        // los archivos presentados se fijan mediante PreregistroRequisitoDocumento.
        public long? PreregistroRequisitoId { get; set; }
        public string Titulo { get; set; } = null!;
        public string EtapaCodigo { get; set; } = null!;
        public string EstadoCodigo { get; set; } = "VIGENTE";
        public long? CreadoPorCuentaId { get; set; }
        public long? CreadoPorUsuarioId { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public Expediente Expediente { get; set; } = null!;
        public TipoDocumento TipoDocumento { get; set; } = null!;
        public PreregistroRequisito? PreregistroRequisito { get; set; }
        public Representacion? RepresentacionPoder { get; set; }
        public SolicitudDisolucion? SolicitudDisolucion { get; set; }
        public ActuacionAdministrativa? ActuacionAdministrativa { get; set; }
        public Pago? Pago { get; set; }
        public Oficio? Oficio { get; set; }
        public CuentaCiudadana? CreadoPorCuenta { get; set; }
        public UsuarioInterno? CreadoPorUsuario { get; set; }
        public ICollection<DocumentoVersion> Versiones { get; set; }
            = new List<DocumentoVersion>();
    }
}
