namespace Divorcios.Negocio.Excepciones
{
    public sealed class LimiteConsultasNegocioException() : Exception("Se alcanzó el límite configurado de consultas de identidad. Intente más tarde.");
}
