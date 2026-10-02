namespace Divorcios.Datos.Resultados
{
    public sealed record RespuestaReniec(
        bool Encontrado, string? Prenombres, string? ApellidoPaterno,
        string? ApellidoMaterno, string? Direccion, short? CodigoHttp,
        string? RespuestaHash);
}
