namespace Divorcios.Negocio.Excepciones
{
    public sealed class ServicioNoDisponibleNegocioException(string codigo, string mensaje) : Exception(mensaje)
    {
        public string Codigo { get; } = codigo;
    }
}
