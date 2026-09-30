using System;

namespace Divorcios.Dominio.Entidades
{
    public class ExpedienteVersion
    {
        public long ExpedienteVersionId { get; set; }
        public long ExpedienteId { get; set; }
        public int NumeroVersion { get; set; }
        public long? PreregistroVersionOrigenId { get; set; }
        public DateOnly FechaMatrimonio { get; set; }
        public bool MatrimonioEnPorvenir { get; set; }
        public bool UltimoDomicilioConyugalPorvenir { get; set; }
        public string? DomicilioConyugal { get; set; }
        public bool TieneHijos { get; set; }
        public short CantidadHijosMenores { get; set; }
        public short CantidadHijosMayores { get; set; }
        // Snapshot de la respuesta declarada, sin información médica.
        public bool? TieneHijosMayoresSituacionEspecial { get; set; }
        public bool TieneBienes { get; set; }
        public bool TieneAcuerdoBienes { get; set; }
        public bool? RequiereRepresentacionA { get; set; }
        public bool? RequiereRepresentacionB { get; set; }
        public string MotivoCambio { get; set; } = null!;
        public long RegistradoPorUsuarioId { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public Expediente Expediente { get; set; } = null!;
        public PreregistroVersion? PreregistroVersionOrigen { get; set; }
        public UsuarioInterno RegistradoPorUsuario { get; set; } = null!;
    }
}
