using System;

namespace Divorcios.Dominio.Entidades
{
    public class Pago
    {
        public long PagoId { get; set; }
        public long ExpedienteId { get; set; }
        public long ExpedienteConyugePaganteId { get; set; }
        public long? DocumentoComprobanteId { get; set; }
        public long RegistradoPorUsuarioId { get; set; }
        public int NumeroPago { get; set; }
        public string DniPaganteSnapshot { get; set; } = null!;
        public string NombrePaganteSnapshot { get; set; } = null!;
        public string ConceptoCodigo { get; set; } = null!;
        public string ConceptoDescripcionSnapshot { get; set; } = null!;
        public decimal Monto { get; set; }
        public string MonedaCodigo { get; set; } = "PEN";
        public string EstadoCodigo { get; set; } = "PENDIENTE";
        // Obligatorio al registrar el pago presencial, incluso si luego se anula.
        public string? NumeroVoucher { get; set; }
        // Dato adicional de caja; no sustituye al número de voucher.
        public string? ReferenciaCaja { get; set; }
        public DateTime SolicitadoEn { get; set; } = DateTime.UtcNow;
        public DateTime? UltimaConsultaCajaEn { get; set; }
        public DateTime? PagadoEn { get; set; }
        public DateTime? AnuladoEn { get; set; }
        public string? Observacion { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public Expediente Expediente { get; set; } = null!;
        public ExpedienteConyuge ExpedienteConyugePagante { get; set; } = null!;
        public Documento? DocumentoComprobante { get; set; }
        public UsuarioInterno RegistradoPorUsuario { get; set; } = null!;
    }
}
