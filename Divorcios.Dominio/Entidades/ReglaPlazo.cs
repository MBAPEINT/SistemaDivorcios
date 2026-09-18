using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class ReglaPlazo
    {
        public short ReglaPlazoId { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public short Cantidad { get; set; }
        public string UnidadCodigo { get; set; } = null!;
        public string TipoDiaCodigo { get; set; } = null!;
        public DateOnly VigenteDesde { get; set; }
        public DateOnly? VigenteHasta { get; set; }
        public string Fuente { get; set; } = null!;
        public ICollection<PlazoCaso> Aplicaciones { get; set; }
            = new List<PlazoCaso>();
    }
}