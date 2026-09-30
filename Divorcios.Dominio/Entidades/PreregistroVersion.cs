using System;
using System.Collections.Generic;

namespace Divorcios.Dominio.Entidades
{
    public class PreregistroVersion
    {
        public long PreregistroVersionId { get; set; }
        public long PreregistroId { get; set; }
        public int NumeroVersion { get; set; }
        public DateOnly FechaMatrimonio { get; set; }
        public bool MatrimonioEnPorvenir { get; set; }
        public bool UltimoDomicilioConyugalPorvenir { get; set; }
        public string? DomicilioConyugal { get; set; }
        public bool TieneHijos { get; set; }
        public short CantidadHijosMenores { get; set; }
        public short CantidadHijosMayores { get; set; }
        // null conserva una respuesta pendiente; no se recopilan datos médicos.
        public bool? TieneHijosMayoresSituacionEspecial { get; set; }
        public bool TieneBienes { get; set; }
        public bool TieneAcuerdoBienes { get; set; }
        // Respuestas declaradas por posición; no acreditan un poder.
        public bool? RequiereRepresentacionA { get; set; }
        public bool? RequiereRepresentacionB { get; set; }
        public string? ObservacionCiudadano { get; set; }
        public string? MotivoCambio { get; set; }
        public long CreadoPorCuentaId { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public Preregistro Preregistro { get; set; } = null!;
        public CuentaCiudadana CreadoPorCuenta { get; set; } = null!;
        public ICollection<PreregistroRequisito> Requisitos { get; set; }
            = new List<PreregistroRequisito>();
        public ICollection<RevisionPreregistro> Revisiones { get; set; }
            = new List<RevisionPreregistro>();
        public ICollection<ExpedienteVersion> VersionesExpedienteOriginadas { get; set; } 
            = new List<ExpedienteVersion>();
        public ICollection<ExpedienteContactoHistorial> ContactosExpedienteOriginados { get; set; }
            = new List<ExpedienteContactoHistorial>();
    }
}
