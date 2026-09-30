using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class EstadoExpediente
    {
        public short EstadoExpedienteId { get; set; }
        public string Codigo { get; set; } = null!;
        public string NombreCiudadano { get; set; } = null!;
        public string EtapaCodigo { get; set; } = null!;
        public short OrdenVisual { get; set; }
        public bool EsFinal { get; set; } = false;
        public bool Activo { get; set; } = true;
        public ICollection<HistorialEstadoExpediente> HistorialExpedientes { get; set; }
            = new List<HistorialEstadoExpediente>();
    }
}