using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class Expediente
    {
        public long ExpedienteId { get; set; }
        public long CasoId { get; set; }
        public string NumeroExpediente { get; set; } = null!;
        public DateOnly FechaIngresoMesaPartes { get; set; }
        public long RegistradoPorUsuarioId { get; set; }
        public string? Observacion { get; set; }
        public DateTime RegistradoEn { get; set; } = DateTime.UtcNow;
        public Caso Caso { get; set; } = null!;
        public UsuarioInterno RegistradoPorUsuario { get; set; } = null!;
    }
}