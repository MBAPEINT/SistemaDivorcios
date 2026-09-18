using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class IntentoNotificacion
    {
        public long IntentoNotificacionId { get; set; }
        public long NotificacionId { get; set; }
        public short NumeroIntento { get; set; }
        public string ResultadoCodigo { get; set; } = "EN_PROCESO";
        public short? CodigoHttp { get; set; }
        public string? ProveedorMensajeId { get; set; }
        public string? Error { get; set; }
        public long? EjecutadoPorUsuarioId { get; set; }
        public DateTime IniciadoEn { get; set; } = DateTime.UtcNow;
        public DateTime? FinalizadoEn { get; set; }
        public Notificacion Notificacion { get; set; } = null!;
        public UsuarioInterno? EjecutadoPorUsuario { get; set; }
    }
}