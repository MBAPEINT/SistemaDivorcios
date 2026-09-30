using System;
using System.Collections.Generic;

namespace Divorcios.Dominio.Entidades
{
    public class ReglaPlazo
    {
        public short ReglaPlazoId { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public short Cantidad { get; set; }
        public string UnidadCodigo { get; set; } = null!;
        public string TipoDiaCodigo { get; set; } = null!;
        public string EventoInicioCodigo { get; set; } = null!;
        public DateOnly VigenteDesde { get; set; }
        public DateOnly? VigenteHasta { get; set; }
        public string Fuente { get; set; } = null!;
        public bool Activo { get; set; } = true;
        public ICollection<PlazoExpediente> Aplicaciones { get; set; }
            = new List<PlazoExpediente>();
    }
}