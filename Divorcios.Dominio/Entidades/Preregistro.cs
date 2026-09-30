using System;
using System.Collections.Generic;

namespace Divorcios.Dominio.Entidades
{
    public class Preregistro
    {
        public long PreregistroId { get; set; }
        public long ExpedienteId { get; set; }
        public string EstadoCodigo { get; set; } = "BORRADOR";
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public DateTime? EnviadoEn { get; set; }
        public DateTime? AprobadoEn { get; set; }
        public DateTime? BloqueadoEn { get; set; }
        public Expediente Expediente { get; set; } = null!;
        public ICollection<PreregistroVersion> Versiones { get; set; }
            = new List<PreregistroVersion>();
    }
}