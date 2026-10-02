using Divorcios.Datos.Interfaces;
using Divorcios.Datos.Opciones;
using Divorcios.Datos.Resultados;
using Divorcios.Dominio.Entidades;
using Divorcios.Negocio.Excepciones;
using Divorcios.Negocio.Extensiones;
using Divorcios.Negocio.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Divorcios.Pruebas
{
    [Collection("PostgreSQL aislado")]
    public sealed class ReniecCachePruebas(PostgresAislado postgres)
    {
        [Fact]
        public async Task DiezIntentosGlobalesRechazanElUndecimoYLaCacheSigueDisponible()
        {
            var ahora = new DateTime(2040, 1, 5, 12, 0, 0, DateTimeKind.Utc);
            await using var db = postgres.CrearContexto();
            db.ConsultasReniec.Add(new ConsultaReniec
            {
                Persona = new Persona { Dni = "80500099", Nombres = "FICTICIO", ApellidoPaterno = "PRUEBA",
                    ApellidoMaterno = "CACHE", VerificadoReniec = true, VerificadoReniecEn = ahora, CreadoEn = ahora },
                DniConsultado = "80500099", Prenombres = "FICTICIO", ApellidoPaterno = "PRUEBA", ApellidoMaterno = "CACHE",
                ResultadoCodigo = "ENCONTRADO", OrigenCodigo = "IMPORTACION", ConsultadoEn = ahora, ExpiraEn = null
            });
            await db.SaveChangesAsync();
            var proveedor = new ProveedorContado(false);
            using var servicios = Servicios(proveedor, ahora);
            for (var numero = 1; numero <= 10; numero++)
            {
                using var scope = servicios.CreateScope();
                await Assert.ThrowsAsync<ServicioNoDisponibleNegocioException>(() => scope.ServiceProvider.GetRequiredService<IIdentidadServicio>()
                    .ConsultarAsync(new() { Dni = (80500000 + numero).ToString() }, default));
            }
            using var siguiente = servicios.CreateScope();
            var servicio = siguiente.ServiceProvider.GetRequiredService<IIdentidadServicio>();
            await Assert.ThrowsAsync<LimiteConsultasNegocioException>(() => servicio.ConsultarAsync(new() { Dni = "80500011" }, default));
            var cache = await servicio.ConsultarAsync(new() { Dni = "80500099" }, default);
            Assert.True(cache.Datos.DesdeCache);
            Assert.Equal(10, proveedor.Llamadas);
            Assert.Equal(10, await db.ConsultasReniec.CountAsync(x => x.OrigenCodigo == "API" && x.ConsultadoEn == ahora));
            Assert.False(await db.ConsultasReniec.AnyAsync(x => x.DniConsultado == "80500011"));
            using var ventanaPosterior = Servicios(proveedor, ahora.AddHours(24).AddSeconds(1));
            using var scopePosterior = ventanaPosterior.CreateScope();
            var errorPosterior = await Assert.ThrowsAsync<ServicioNoDisponibleNegocioException>(() => scopePosterior.ServiceProvider
                .GetRequiredService<IIdentidadServicio>().ConsultarAsync(new() { Dni = "80500012" }, default));
            Assert.Equal("RENIEC_RESPUESTA_NO_VERIFICABLE", errorPosterior.Codigo);
            Assert.Equal(11, proveedor.Llamadas);
        }

        [Fact]
        public async Task ResultadoVerificadoSinCaducidadSeGuardaYReutilizaTrasReiniciarServicio()
        {
            var proveedor = new ProveedorContado(true);
            using (var servicios = Servicios(proveedor, new DateTime(2041, 1, 5, 12, 0, 0, DateTimeKind.Utc)))
            using (var scope = servicios.CreateScope())
            {
                var primera = await scope.ServiceProvider.GetRequiredService<IIdentidadServicio>().ConsultarAsync(new() { Dni = "80500100" }, default);
                Assert.False(primera.Datos.DesdeCache);
            }
            using (var reiniciado = Servicios(proveedor, new DateTime(2042, 1, 5, 12, 0, 0, DateTimeKind.Utc)))
            using (var scope = reiniciado.CreateScope())
            {
                var segunda = await scope.ServiceProvider.GetRequiredService<IIdentidadServicio>().ConsultarAsync(new() { Dni = "80500100" }, default);
                Assert.True(segunda.Datos.DesdeCache);
            }
            Assert.Equal(1, proveedor.Llamadas);
            await using var db = postgres.CrearContexto();
            Assert.Null((await db.ConsultasReniec.SingleAsync(x => x.DniConsultado == "80500100")).ExpiraEn);
            Assert.Equal(1, await db.Personas.CountAsync(x => x.Dni == "80500100"));
        }

        private ServiceProvider Servicios(IReniecProveedor proveedor, DateTime ahora)
        {
            var servicios = new ServiceCollection();
            servicios.AddLogging();
            servicios.AgregarCapaNegocio(postgres.Conexion);
            servicios.AddSingleton(proveedor);
            servicios.AddSingleton<TimeProvider>(new RelojFijo(ahora));
            servicios.Configure<ReniecOpciones>(x =>
            {
                x.Habilitado = true; x.Usuario = "ficticio"; x.Clave = "ficticia";
                x.LimiteDiario = 10; x.VigenciaCacheMinutos = null;
            });
            return servicios.BuildServiceProvider();
        }

        private sealed class RelojFijo(DateTime ahora) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => new(ahora);
        }
        private sealed class ProveedorContado(bool encontrado) : IReniecProveedor
        {
            public int Llamadas { get; private set; }
            public Task<RespuestaReniec> ConsultarAsync(string dni, CancellationToken cancellationToken)
            {
                Llamadas++;
                return Task.FromResult(encontrado
                    ? new RespuestaReniec(true, "FICTICIO", "PRUEBA", "CACHE", "CALLE FICTICIA 123", 200, null)
                    : new RespuestaReniec(false, null, null, null, null, 200, null));
            }
        }
    }
}
