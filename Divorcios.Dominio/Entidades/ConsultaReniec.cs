using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class ConsultaReniec
    {
        public long ConsultaReniecId { get; set; }
        public string DniConsultado { get; set; } = null!;
        public string? Prenombres { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public string? Direccion { get; set; }
        public string ResultadoCodigo { get; set; } = null!;
        public short? CodigoHttp { get; set; }
        public DateTime ConsultadoEn { get; set; } = DateTime.UtcNow;
        public DateTime ExpiraEn { get; set; }
        public long? PersonaId { get; set; }
        public Persona? Persona { get; set; }
    }
}