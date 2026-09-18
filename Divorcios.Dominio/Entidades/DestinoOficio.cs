using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class DestinoOficio
    {
        public short DestinoOficioId { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; } = true;
        public ICollection<Oficio> Oficios { get; set; }
            = new List<Oficio>();
    }
}