using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class RequisitoCatalogo
    {
        public short RequisitoCatalogoId { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;
        public ICollection<PreregistroRequisito> PrerregistrosRequisitos { get; set; }
            = new List<PreregistroRequisito>();
    }
}