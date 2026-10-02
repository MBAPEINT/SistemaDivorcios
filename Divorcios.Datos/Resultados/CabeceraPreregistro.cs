using Divorcios.Dominio.Entidades;

namespace Divorcios.Datos.Resultados
{
    public sealed record ReferenciaVersionPreregistro(long VersionId, int NumeroVersion, DateTime CreadoEn);
    public sealed record ReferenciaRevisionPreregistro(long RevisionId, long VersionId, string ResultadoCodigo,
        DateTime IniciadaEn, DateTime? FinalizadaEn);
    public sealed record CabeceraPreregistro(Preregistro Registro, IReadOnlyList<ReferenciaVersionPreregistro> Versiones,
        IReadOnlyList<ReferenciaRevisionPreregistro> Revisiones, string? VersionEnviadaAuditoriaId);
    public sealed record PaginaPreregistros(IReadOnlyList<Preregistro> Items, long Total);
}
