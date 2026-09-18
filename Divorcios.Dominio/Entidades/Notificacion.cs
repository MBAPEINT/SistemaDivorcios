using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class Notificacion
    {
        public long NotificacionId { get; set; }
        public long CasoId { get; set; }
        public long PersonaDestinatariaId { get; set; }
        public string CanalCodigo { get; set; } = "WHATSAPP";
        public string TipoCodigo { get; set; } = null!;
        public string Destino { get; set; } = null!;
        public string? PlantillaCodigo { get; set; }
        public string Contenido { get; set; } = null!;
        public string EstadoCodigo { get; set; } = "PENDIENTE";
        public DateTime? ProximoIntentoEn { get; set; }
        public DateTime? FinalizadaEn { get; set; }
        public long? CreadaPorUsuarioId { get; set; }
        public DateTime CreadaEn { get; set; } = DateTime.UtcNow;
        public Caso Caso { get; set; } = null!;
        public Persona PersonaDestinataria { get; set; } = null!;
        public UsuarioInterno? CreadaPorUsuario { get; set; }
        public ICollection<IntentoNotificacion> Intentos { get; set; }
            = new List<IntentoNotificacion>();
    }
}