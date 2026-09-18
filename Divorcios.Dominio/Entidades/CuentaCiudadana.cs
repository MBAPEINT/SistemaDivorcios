using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class CuentaCiudadana
    {
        public long CuentaCiudadanaId { get; set; }
        public long PersonaId { get; set; }
        public DateTime? CelularVerificadoEn { get; set; }
        public DateTime? BloqueadoEn { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public Persona Persona { get; set; } = null!;
        public ICollection<ValidacionIdentidad> ValidacionesIdentidad { get; set; }
            = new List<ValidacionIdentidad>();
        public ICollection<Caso> CasosCreados { get; set; } 
            = new List<Caso>();
        public ICollection<DocumentoVersion> VersionesDocumentoCargadas { get; set; }
            = new List<DocumentoVersion>();
        public ICollection<RegistroAuditoria> RegistrosAuditoria { get; set; }
            = new List<RegistroAuditoria>();
    }
}
