using Divorcios.Datos.Contexto;
using Microsoft.EntityFrameworkCore;
using Divorcios.Datos.Interfaces;
using Divorcios.Datos.Repositorios;
using Microsoft.Extensions.DependencyInjection;

namespace Divorcios.Datos.Extensiones
{
    public static class InyeccionDependencias
    {
        public static IServiceCollection AgregarCapaDatos(
            this IServiceCollection servicios,
            string cadenaConexion)
        {
            servicios.AddDbContext<DivorciosDbContext>(opciones =>
            {
                opciones.UseNpgsql(cadenaConexion);
                opciones.UseSnakeCaseNamingConvention();
            });

            servicios.AddScoped<ICatalogosRepositorio, CatalogosRepositorio>();

            return servicios;
        }
    }
}