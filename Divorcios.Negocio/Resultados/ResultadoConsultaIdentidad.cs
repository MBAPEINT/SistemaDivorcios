using Divorcios.Negocio.DTOs.Identidad;

namespace Divorcios.Negocio.Resultados
{
    // La dirección se usa dentro de Negocio para el acceso provisional; no se devuelve en P02.
    public sealed record ResultadoConsultaIdentidad(ConsultaIdentidadDto Datos, string? Direccion);
}
