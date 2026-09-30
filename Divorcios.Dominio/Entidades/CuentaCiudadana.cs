using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class CuentaCiudadana
    {
        public long CuentaCiudadanaId { get; set; }
        public long PersonaId { get; set; }
        public DateTime? CorreoVerificadoEn { get; set; }
        public string? ClaveHash { get; set; }
        public string EstadoCodigo { get; set; } = "ACTIVA";
        public short IntentosFallidos { get; set; }
        public DateTime? BloqueadoHasta { get; set; }
        public DateTime? UltimoAccesoEn { get; set; }
        public DateTime? CelularVerificadoEn { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public Persona Persona { get; set; } = null!;
        public ICollection<ValidacionIdentidad> ValidacionesIdentidad { get; set; }
            = new List<ValidacionIdentidad>();
        public ICollection<Expediente> ExpedientesCreados { get; set; }
            = new List<Expediente>();
        public ICollection<DocumentoVersion> VersionesDocumentoCargadas { get; set; }
            = new List<DocumentoVersion>();
        public ICollection<RegistroAuditoria> RegistrosAuditoria { get; set; }
            = new List<RegistroAuditoria>();
        public ICollection<PreregistroVersion> VersionesPreregistroCreadas { get; set; } 
            = new List<PreregistroVersion>();
        public ICollection<Documento> DocumentosCreados { get; set; }
            = new List<Documento>();
    }
}
