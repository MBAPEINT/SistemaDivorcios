using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class EstadoCaso
    {
        public short EstadoCasoId { get; set; }
        public string Codigo { get; set; } = null!;
        public string NombreCiudadano { get; set; } = null!;
        public string EtapaCodigo { get; set; } = null!;
        public short OrdenVisual { get; set; }
        public bool EsFinal { get; set; } = false;
        public bool Activo { get; set; } = true;
        public ICollection<HistorialEstadoCaso> HistorialCasos { get; set; }
            = new List<HistorialEstadoCaso>();
    }
}