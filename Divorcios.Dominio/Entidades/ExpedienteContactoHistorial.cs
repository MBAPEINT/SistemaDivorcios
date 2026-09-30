using System;

namespace Divorcios.Dominio.Entidades
{
    public class ExpedienteContactoHistorial
    {
        public long ExpedienteContactoHistorialId { get; set; }
        public long ExpedienteConyugeId { get; set; }
        public string TipoContactoCodigo { get; set; } = null!;
        public string Valor { get; set; } = null!;
        public string FuenteCodigo { get; set; } = null!;
        public long? PreregistroVersionOrigenId { get; set; }
        public DateTime VigenteDesde { get; set; } = DateTime.UtcNow;
        public DateTime? VigenteHasta { get; set; }
        public long? RegistradoPorUsuarioId { get; set; }
        public ExpedienteConyuge ExpedienteConyuge { get; set; } = null!;
        public PreregistroVersion? PreregistroVersionOrigen { get; set; }
        public UsuarioInterno? RegistradoPorUsuario { get; set; }
    }
}