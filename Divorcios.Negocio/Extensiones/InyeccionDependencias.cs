using Divorcios.Datos.Extensiones;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Servicios;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Divorcios.Negocio.Opciones;
using Divorcios.Datos.Opciones;

namespace Divorcios.Negocio.Extensiones
{
    public static class InyeccionDependencias
    {
        public static IServiceCollection ConfigurarPreregistro(this IServiceCollection servicios, IConfiguration configuracion)
        {
            servicios.Configure<ReniecOpciones>(configuracion.GetSection(ReniecOpciones.Seccion));
            servicios.Configure<InformacionPreregistroOpciones>(configuracion.GetSection(InformacionPreregistroOpciones.Seccion));
            servicios.Configure<AccesoCiudadanoOpciones>(configuracion.GetSection(AccesoCiudadanoOpciones.Seccion));
            servicios.Configure<ArchivosPreregistroOpciones>(configuracion.GetSection(ArchivosPreregistroOpciones.Seccion));
            servicios.Configure<MatrizRequisitosOpciones>(configuracion.GetSection(MatrizRequisitosOpciones.Seccion));
            servicios.Configure<AlmacenamientoArchivosOpciones>(configuracion.GetSection(AlmacenamientoArchivosOpciones.Seccion));
            return servicios;
        }

        public static IServiceCollection AgregarCapaNegocio(
            this IServiceCollection servicios, string cadenaConexion)
        {
            servicios.AgregarCapaDatos(cadenaConexion);
            servicios.AddScoped<ICatalogosServicio, CatalogosServicio>();
            servicios.AddScoped<IInformacionPreregistroServicio, InformacionPreregistroServicio>();
            servicios.AddScoped<IIdentidadServicio, IdentidadServicio>();
            servicios.AddScoped<IAccesoCiudadanoServicio, AccesoCiudadanoServicio>();
            servicios.AddScoped<IPreregistrosServicio, PreregistrosServicio>();
            servicios.AddScoped<IVersionesPreregistroServicio, VersionesPreregistroServicio>();
            servicios.AddScoped<IArchivosPreregistroServicio, ArchivosPreregistroServicio>();
            servicios.AddScoped<IGeneracionRequisitosServicio, GeneracionRequisitosServicio>();
            servicios.AddSingleton(TimeProvider.System);
            return servicios;
        }
    }
}
