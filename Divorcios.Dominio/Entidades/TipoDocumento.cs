using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class TipoDocumento
    {
        public short TipoDocumentoId { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string OrigenCodigo { get; set; } = null!;
        public bool Activo { get; set; } = true;
        public ICollection<Documento> Documentos { get; set; }
            = new List<Documento>();
    }
}