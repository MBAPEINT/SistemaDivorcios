using System;
using System.Collections.Generic;
using System.Text;

namespace Divorcios.Dominio.Entidades
{
    public class CasoConyuge
    {
        public long CasoConyugeId { get; set; }
        public long CasoId { get; set; }
        public long PersonaId { get; set; }
        public string PosicionCodigo { get; set; } = null!;
        public Caso Caso { get; set; } = null!;
        public Persona Persona { get; set; } = null!;
        public Representacion? Representacion { get; set; }
        public ICollection<AsistenciaAudiencia> AsistenciasAudiencia { get; set; }
            = new List<AsistenciaAudiencia>();
        public ICollection<SolicitanteDisolucion> SolicitudesDisolucionPresentadas { get; set; }
            = new List<SolicitanteDisolucion>();
    }
}
