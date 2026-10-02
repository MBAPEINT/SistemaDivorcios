namespace Divorcios.Negocio.Excepciones
{
    public sealed class ArchivoNegocioException(int estadoHttp, string codigo, string mensaje) : Exception(mensaje)
    {
        public int EstadoHttp { get; } = estadoHttp;
        public string Codigo { get; } = codigo;
    }
}
