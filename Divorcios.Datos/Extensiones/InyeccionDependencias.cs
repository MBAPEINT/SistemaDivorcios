using Divorcios.Datos.Contexto;
using Microsoft.EntityFrameworkCore;
using Divorcios.Datos.Interfaces;
using Divorcios.Datos.Repositorios;
using Microsoft.Extensions.DependencyInjection;
using Divorcios.Datos.Integraciones;

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
            servicios.AddScoped<IIdentidadRepositorio, IdentidadRepositorio>();
            servicios.AddScoped<IAccesoCiudadanoRepositorio, AccesoCiudadanoRepositorio>();
            servicios.AddScoped<IPreregistrosRepositorio, PreregistrosRepositorio>();
            servicios.AddScoped<IVersionesPreregistroRepositorio, VersionesPreregistroRepositorio>();
            servicios.AddScoped<IArchivosPreregistroRepositorio, ArchivosPreregistroRepositorio>();
            servicios.AddScoped<IGeneracionRequisitosRepositorio, GeneracionRequisitosRepositorio>();
            servicios.AddScoped<IAlmacenamientoArchivos, Divorcios.Datos.Almacenamiento.AlmacenamientoArchivosLocal>();
            servicios.AddHttpClient<IReniecProveedor, ReniecProveedor>(cliente =>
            {
                cliente.Timeout = Timeout.InfiniteTimeSpan;
            }).RemoveAllLoggers().ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // No reenviar credenciales siguiendo redirecciones del proveedor.
                AllowAutoRedirect = false
            });

            return servicios;
        }
    }
}
