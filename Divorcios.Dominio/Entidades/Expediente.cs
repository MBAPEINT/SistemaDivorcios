using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class Expediente
    {
        public long ExpedienteId { get; set; }
        public string CodigoPreregistro { get; set; } = null!;
        public string? NumeroExpediente { get; set; }
        public long? CreadoPorCuentaId { get; set; }
        public DateTime FechaInicioDigital { get; set; } = DateTime.UtcNow;
        public DateOnly? FechaIngresoMesaPartes { get; set; }
        public DateTime? OficializadoEn { get; set; }
        public DateTime? PreregistroBloqueadoEn { get; set; }
        public DateTime? CerradoEn { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public CuentaCiudadana? CreadoPorCuenta { get; set; }
        public Preregistro? Preregistro { get; set; }
        public SolicitudDisolucion? SolicitudDisolucion { get; set; }
        public ICollection<ExpedienteConyuge> Conyuges { get; set; }
            = new List<ExpedienteConyuge>();
        public ICollection<Documento> Documentos { get; set; }
            = new List<Documento>();
        public ICollection<HistorialEstadoExpediente> HistorialEstados
        { get; set; } = new List<HistorialEstadoExpediente>();
        public ICollection<PlazoExpediente> Plazos { get; set; }
            = new List<PlazoExpediente>();
        public ICollection<AudienciaRatificacion> AudienciasRatificacion
        { get; set; } = new List<AudienciaRatificacion>();
        public ICollection<ActuacionAdministrativa> ActuacionesAdministrativas
        { get; set; } = new List<ActuacionAdministrativa>();
        public ICollection<Pago> Pagos { get; set; }
            = new List<Pago>();
        public ICollection<Notificacion> Notificaciones { get; set; }
            = new List<Notificacion>();
        public ICollection<RegistroAuditoria> RegistrosAuditoria { get; set; }
            = new List<RegistroAuditoria>();
        public ICollection<ExpedienteVersion> Versiones { get; set; }
            = new List<ExpedienteVersion>();
    }
}