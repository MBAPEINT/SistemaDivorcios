using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class ValidacionIdentidad
    {
        public long ValidacionIdentidadId { get; set; }
        public long CuentaCiudadanaId { get; set; }
        public string ProveedorCodigo { get; set; } = "RENIEC";
        public string? ReferenciaConsulta { get; set; }
        public string ResultadoCodigo { get; set; } = null!;
        public DateTime ValidadoEn { get; set; } = DateTime.UtcNow;
        public CuentaCiudadana CuentaCiudadana { get; set; } = null!;
    }
}
