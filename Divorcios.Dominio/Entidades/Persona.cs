using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class Persona
    {
        public long PersonaId { get; set; }
        public string Dni { get; set; } = null!;
        public string Nombres { get; set; } = null!;
        public string ApellidoPaterno { get; set; } = null!;
        public string ApellidoMaterno { get; set; } = null!;
        public string? Celular { get; set; }
        public string? Correo { get; set; }
        public string? DireccionDni { get; set; }
        public bool VerificadoReniec { get; set; }
        public DateTime? VerificadoReniecEn { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public DateTime? ActualizadoEn { get; set; }
        public CuentaCiudadana? CuentaCiudadana { get; set; }
        public ICollection<ExpedienteConyuge> ParticipacionesExpediente { get; set; }
            = new List<ExpedienteConyuge>();
        public ICollection<ConsultaReniec> ConsultasReniec { get; set; }
            = new List<ConsultaReniec>();
        public ICollection<Representacion> RepresentacionesComoApoderado { get; set; }
            = new List<Representacion>();
        public ICollection<Notificacion> NotificacionesRecibidas { get; set; }
            = new List<Notificacion>();
    }
}
