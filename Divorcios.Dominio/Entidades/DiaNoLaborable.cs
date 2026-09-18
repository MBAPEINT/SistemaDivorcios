using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class DiaNoLaborable
    {
        public int DiaNoLaborableId { get; set; }
        public DateOnly Fecha { get; set; }
        public string Nombre { get; set; } = null!;
        public string TipoCodigo { get; set; } = null!;
        public string AmbitoCodigo { get; set; } = null!;
        public bool ExcluyeDiaHabil { get; set; }
        public bool ExcluyeDiaOperativo { get; set; }
        public bool Activo { get; set; } = true;
        public long? RegistradoPorUsuarioId { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public UsuarioInterno? RegistradoPorUsuario { get; set; }
    }
}