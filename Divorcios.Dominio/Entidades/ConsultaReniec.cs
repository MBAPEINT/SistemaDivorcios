using System;

namespace Divorcios.Dominio.Entidades
{
    public class ConsultaReniec
    {
        public long ConsultaReniecId { get; set; }
        public long? PersonaId { get; set; }
        public string DniConsultado { get; set; } = null!;
        public string? Prenombres { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public string? Direccion { get; set; }
        public string ResultadoCodigo { get; set; } = null!;
        public string OrigenCodigo { get; set; } = "API";
        public short? CodigoHttp { get; set; }
        public string? RespuestaHash { get; set; }
        public DateTime ConsultadoEn { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiraEn { get; set; }
        public Persona? Persona { get; set; }
    }
}