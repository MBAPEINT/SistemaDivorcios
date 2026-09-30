using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class Oficio
    {
        public long OficioId { get; set; }
        public long ActuacionAdministrativaId { get; set; }
        public short DestinoOficioId { get; set; }
        public long? DocumentoOficioId { get; set; }
        public string EstadoCodigo { get; set; } = "BORRADOR";
        public string? NumeroOficio { get; set; }
        public DateOnly? FechaEmision { get; set; }
        public DateTime? EnviadoEn { get; set; }
        public DateTime? RecibidoEn { get; set; }
        public long RegistradoPorUsuarioId { get; set; }
        public DateTime RegistradoEn { get; set; } = DateTime.UtcNow;
        public string? Observacion { get; set; }
        public ActuacionAdministrativa ActuacionAdministrativa { get; set; }
            = null!;
        public DestinoOficio DestinoOficio { get; set; } = null!;
        public Documento? DocumentoOficio { get; set; } = null!;
        public UsuarioInterno RegistradoPorUsuario { get; set; } = null!;
    }
}