using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class Caso
    {
        public long CasoId { get; set; }
        public string CodigoPre { get; set; } = null!;
        public long? CreadoPorCuentaId { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public DateTime? CerradoEn { get; set; }
        public CuentaCiudadana? CreadoPorCuenta { get; set; }
        public Preregistro? Preregistro { get; set; }
        public Expediente? Expediente { get; set; }
        public SolicitudDisolucion? SolicitudDisolucion { get; set; }
        public ICollection<CasoConyuge> Conyuges { get; set; }
            = new List<CasoConyuge>();
        public ICollection<Documento> Documentos { get; set; }
            = new List<Documento>();
        public ICollection<HistorialEstadoCaso> HistorialEstados { get; set; }
            = new List<HistorialEstadoCaso>();
        public ICollection<PlazoCaso> Plazos { get; set; }
            = new List<PlazoCaso>();
        public ICollection<AudienciaRatificacion> AudienciasRatificacion { get; set; }
            = new List<AudienciaRatificacion>();
        public ICollection<ActuacionAdministrativa> ActuacionesAdministrativas { get; set; }
            = new List<ActuacionAdministrativa>();
        public ICollection<PagoTramite> Pagos { get; set; }
            = new List<PagoTramite>();
        public ICollection<Notificacion> Notificaciones { get; set; }
            = new List<Notificacion>();
        public ICollection<RegistroAuditoria> RegistrosAuditoria { get; set; }
            = new List<RegistroAuditoria>();
    }
}
