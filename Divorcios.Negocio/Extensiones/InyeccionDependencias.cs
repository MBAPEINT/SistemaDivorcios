using Divorcios.Datos.Extensiones;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Servicios;
using Microsoft.Extensions.DependencyInjection;

namespace Divorcios.Negocio.Extensiones
{
    public static class InyeccionDependencias
    {
        public static IServiceCollection AgregarCapaNegocio(
            this IServiceCollection servicios, string cadenaConexion)
        {
            servicios.AgregarCapaDatos(cadenaConexion);
            servicios.AddScoped<ICatalogosServicio, CatalogosServicio>();
            return servicios;
        }
    }
}
