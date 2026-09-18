using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class PagoTramite
    {
        public long PagoTramiteId { get; set; }
        public long CasoId { get; set; }
        public string ConceptoCodigo { get; set; } = null!;
        public string NumeroVoucher { get; set; } = null!;
        public decimal Monto { get; set; }
        public string MonedaCodigo { get; set; } = "PEN";
        public DateOnly FechaPago { get; set; }
        public long? DocumentoComprobanteId { get; set; }
        public long RegistradoPorUsuarioId { get; set; }
        public DateTime RegistradoEn { get; set; } = DateTime.UtcNow;
        public string? Observacion { get; set; }
        public Caso Caso { get; set; } = null!;
        public Documento? DocumentoComprobante { get; set; }
        public UsuarioInterno RegistradoPorUsuario { get; set; } = null!;
    }
}