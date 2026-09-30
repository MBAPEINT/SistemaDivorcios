using System;
using System.Collections.Generic;

namespace Divorcios.Dominio.Entidades
{
    public class ActuacionAdministrativa
    {
        public long ActuacionAdministrativaId { get; set; }
        public long ExpedienteId { get; set; }
        public long? DocumentoId { get; set; }
        public string TipoCodigo { get; set; } = null!;
        public int NumeroSecuencia { get; set; }
        public string EstadoCodigo { get; set; } = "BORRADOR";
        public string? NumeroResolucion { get; set; }
        public DateOnly? FechaEmision { get; set; }
        public DateOnly? FechaNotificacion { get; set; }
        public long CreadaPorUsuarioId { get; set; }
        public DateTime CreadaEn { get; set; } = DateTime.UtcNow;
        public long? EmitidaPorUsuarioId { get; set; }
        public DateTime? EmitidaEn { get; set; }
        public string? Observacion { get; set; }
        public Expediente Expediente { get; set; } = null!;
        public Documento? Documento { get; set; }
        public UsuarioInterno CreadaPorUsuario { get; set; } = null!;
        public UsuarioInterno? EmitidaPorUsuario { get; set; }
        public ICollection<Oficio> Oficios { get; set; }
            = new List<Oficio>();
    }
}