using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class UsuarioInterno
    {
        public long UsuarioInternoId { get; set; }
        public string Login { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string NombreVisible { get; set; } = null!;
        public string RolCodigo { get; set; } = null!;
        public bool Activo { get; set; } = true;
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public DateTime? UltimoAccesoEn { get; set; }
        public ICollection<DocumentoVersion> VersionesDocumentoCargadas { get; set; }
            = new List<DocumentoVersion>();
        public ICollection<RevisionPreregistro> RevisionesPrerregistro { get; set; }
            = new List<RevisionPreregistro>();
        public ICollection<HistorialEstadoExpediente> EstadosExpedienteRegistrados { get; set; }
            = new List<HistorialEstadoExpediente>();
        public ICollection<PlazoExpediente> PlazosCreados { get; set; }
            = new List<PlazoExpediente>();
        public ICollection<AudienciaRatificacion> AudienciasCreadas { get; set; }
            = new List<AudienciaRatificacion>();
        public ICollection<SolicitudDisolucion> SolicitudesDisolucionRegistradas { get; set; }
            = new List<SolicitudDisolucion>();
        public ICollection<SolicitudDisolucion> SolicitudesDisolucionValidadas { get; set; }
            = new List<SolicitudDisolucion>();
        public ICollection<ActuacionAdministrativa> ActuacionesAdministrativasCreadas { get; set; }
            = new List<ActuacionAdministrativa>();
        public ICollection<ActuacionAdministrativa> ActuacionesAdministrativasEmitidas { get; set; }
            = new List<ActuacionAdministrativa>();
        public ICollection<Oficio> OficiosRegistrados { get; set; }
            = new List<Oficio>();
        public ICollection<Pago> PagosRegistrados { get; set; }
            = new List<Pago>();
        public ICollection<Notificacion> NotificacionesCreadas { get; set; }
            = new List<Notificacion>();
        public ICollection<IntentoNotificacion> IntentosNotificacionEjecutados { get; set; }
            = new List<IntentoNotificacion>();
        public ICollection<DiaNoLaborable> DiasNoLaborablesRegistrados { get; set; }
            = new List<DiaNoLaborable>();
        public ICollection<RegistroAuditoria> RegistrosAuditoria { get; set; }
            = new List<RegistroAuditoria>();
        public ICollection<ExpedienteVersion> VersionesExpedienteRegistradas { get; set; } 
            = new List<ExpedienteVersion>();
        public ICollection<ExpedienteContactoHistorial> ContactosExpedienteRegistrados { get; set; }
            = new List<ExpedienteContactoHistorial>();
        public ICollection<Documento> DocumentosCreados { get; set; }
            = new List<Documento>();
    }
}
