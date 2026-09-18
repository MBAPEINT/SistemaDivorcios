using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class ActuacionAdministrativa
    {
        public long ActuacionAdministrativaId { get; set; }
        public long CasoId { get; set; }
        public string TipoCodigo { get; set; } = null!;
        public string EstadoCodigo { get; set; } = "BORRADOR";
        public string? NumeroResolucion { get; set; }
        public DateOnly? FechaEmision { get; set; }
        public long CreadaPorUsuarioId { get; set; }
        public DateTime CreadaEn { get; set; } = DateTime.UtcNow;
        public long? EmitidaPorUsuarioId { get; set; }
        public DateTime? EmitidaEn { get; set; }
        public string? Observacion { get; set; }
        public Caso Caso { get; set; } = null!;
        public UsuarioInterno CreadaPorUsuario { get; set; } = null!;
        public UsuarioInterno? EmitidaPorUsuario { get; set; }
        public ICollection<Documento> Documentos { get; set; }
            = new List<Documento>();
        public ICollection<Oficio> Oficios { get; set; }
            = new List<Oficio>();
    }
}