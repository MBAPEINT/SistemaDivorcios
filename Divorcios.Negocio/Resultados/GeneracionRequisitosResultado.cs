using Divorcios.Negocio.Matrices;

namespace Divorcios.Negocio.Resultados
{
    public sealed record RequisitoGenerado(string PreregistroRequisitoId, DefinicionRequisito Definicion);
    public sealed record GeneracionRequisitosResultado(string EstadoCodigo, string? MatrizVersion,
        IReadOnlyList<string> Pendientes, IReadOnlyList<RequisitoGenerado> RequisitosGenerados);
}
