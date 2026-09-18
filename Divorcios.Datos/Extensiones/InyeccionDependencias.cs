using Divorcios.Datos.Contexto;
using Microsoft.EntityFrameworkCore;
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

            return servicios;
        }
    }
}