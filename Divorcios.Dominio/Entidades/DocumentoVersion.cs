using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class DocumentoVersion
    {
        public long DocumentoVersionId { get; set; }
        public long DocumentoId { get; set; }
        public short NumeroVersion { get; set; }
        public string NombreArchivo { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public long TamanoBytes { get; set; }
        public string AlmacenamientoClave { get; set; } = null!;
        public string Sha256 { get; set; } = null!;
        public long? CargadoPorCuentaId { get; set; }
        public long? CargadoPorUsuarioId { get; set; }
        public DateTime CargadoEn { get; set; } = DateTime.UtcNow;
        public Documento Documento { get; set; } = null!;
        public CuentaCiudadana? CargadoPorCuenta { get; set; }
        public UsuarioInterno? CargadoPorUsuario { get; set; }
    }
}