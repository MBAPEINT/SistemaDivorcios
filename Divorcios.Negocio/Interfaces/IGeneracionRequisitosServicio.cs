using Divorcios.Dominio.Entidades;
using Divorcios.Negocio.Resultados;

namespace Divorcios.Negocio.Interfaces
{
    public interface IGeneracionRequisitosServicio
    {
        // Coordinado por V01 dentro de su transacción; no es un endpoint para recalcular historias.
        Task<GeneracionRequisitosResultado> GenerarAsync(PreregistroVersion version, CancellationToken cancellationToken);
    }
}
