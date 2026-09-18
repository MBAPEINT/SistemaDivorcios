using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class RegistroAuditoria
    {
        public long RegistroAuditoriaId { get; set; }
        public string ActorTipoCodigo { get; set; } = null!;
        public long? UsuarioInternoId { get; set; }
        public long? CuentaCiudadanaId { get; set; }
        public long? CasoId { get; set; }
        public string AccionCodigo { get; set; } = null!;
        public string RecursoCodigo { get; set; } = null!;
        public string? RecursoId { get; set; }
        public string ResultadoCodigo { get; set; } = "EXITO";
        public string? Descripcion { get; set; }
        public string? DetalleJson { get; set; }
        public string? DireccionIp { get; set; }
        public string? UserAgent { get; set; }
        public Guid CorrelacionId { get; set; } = Guid.NewGuid();
        public DateTime RegistradoEn { get; set; } = DateTime.UtcNow;
        public UsuarioInterno? UsuarioInterno { get; set; }
        public CuentaCiudadana? CuentaCiudadana { get; set; }
        public Caso? Caso { get; set; }
    }
}