using Divorcios.Dominio.Entidades;

namespace Divorcios.Datos.Resultados
{
    public sealed record PaginaDocumentosPreregistro(IReadOnlyList<Documento> Items, long Total);
}
