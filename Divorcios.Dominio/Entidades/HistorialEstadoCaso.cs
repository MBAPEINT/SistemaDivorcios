using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class HistorialEstadoCaso
    {
        public long HistorialEstadoCasoId { get; set; }
        public long CasoId { get; set; }
        public short EstadoCasoId { get; set; }
        public int NumeroSecuencia { get; set; }
        public long? RegistradoPorUsuarioId { get; set; }
        public DateTime IniciadoEn { get; set; }
        public DateTime? FinalizadoEn { get; set; }
        public DateTime RegistradoEn { get; set; } = DateTime.UtcNow;
        public string? Observacion { get; set; }
        public Caso Caso { get; set; } = null!;
        public EstadoCaso EstadoCaso { get; set; } = null!;
        public UsuarioInterno? RegistradoPorUsuario { get; set; }
        public ICollection<PlazoCaso> PlazosOriginados { get; set; }
            = new List<PlazoCaso>();
    }
}