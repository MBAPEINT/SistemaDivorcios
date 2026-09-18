using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class Preregistro
    {
        public long PreregistroId { get; set; }
        public long CasoId { get; set; }
        public DateOnly FechaMatrimonio { get; set; }
        public bool MatrimonioEnPorvenir { get; set; }
        public bool UltimoDomicilioConyugalEnPorvenir { get; set; }
        public bool MutuoAcuerdoDeclarado { get; set; }
        public short CantidadHijosMenores { get; set; }
        public short CantidadHijosMayoresIncapaces { get; set; }
        public bool TieneBienesSociales { get; set; }
        public DateTime? EnviadoEn { get; set; }
        public DateTime? AprobadoEn { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public Caso Caso { get; set; } = null!;
        public ICollection<PreregistroRequisito> Requisitos { get; set; }
            = new List<PreregistroRequisito>();
        public ICollection<RevisionPreregistro> Revisiones { get; set; }
            = new List<RevisionPreregistro>();
    }
}