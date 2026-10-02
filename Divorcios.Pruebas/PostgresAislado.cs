using System.Diagnostics;
using Divorcios.Datos.Contexto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace Divorcios.Pruebas
{

    [CollectionDefinition("PostgreSQL aislado")]
    public sealed class ColeccionPostgres : ICollectionFixture<PostgresAislado>;

    public sealed class PostgresAislado : IAsyncLifetime
    {
        private readonly string nombre = "divorcios-pruebas-a-" + Guid.NewGuid().ToString("N")[..10];
        public string Conexion { get; private set; } = "";

        public async Task InitializeAsync()
        {
            var clave = Guid.NewGuid().ToString("N");
            try
            {
                Docker("run", "--detach", "--rm", "--name", nombre,
                    "--label", "divorcios.pruebas=paquete-a", "--publish", "127.0.0.1::5432",
                    "--tmpfs", "/var/lib/postgresql/data:rw", "--env", "POSTGRES_DB=divorcios_pruebas_a",
                    "--env", "POSTGRES_USER=pruebas", "--env", "POSTGRES_PASSWORD=" + clave, "postgres:17-alpine");
                var puerto = int.Parse(Docker("port", nombre, "5432/tcp").Trim().Split(':')[^1]);
                Conexion = new NpgsqlConnectionStringBuilder
                {
                    Host = "127.0.0.1", Port = puerto, Database = "divorcios_pruebas_a", Username = "pruebas",
                    Password = clave, Timeout = 1, Pooling = false
                }.ConnectionString;
                for (var intento = 0; ; intento++)
                {
                    try
                    {
                        await using var conexion = new NpgsqlConnection(Conexion);
                        await conexion.OpenAsync();
                        break;
                    }
                    catch (NpgsqlException) when (intento < 40) { await Task.Delay(500); }
                }
                await using var contexto = CrearContexto();
                await contexto.Database.MigrateAsync();
            }
            catch
            {
                await DisposeAsync();
                throw;
            }
        }

        public DivorciosDbContext CrearContexto(IInterceptor? interceptor = null)
        {
            var conexion = new NpgsqlConnectionStringBuilder(Conexion);
            if (conexion.Host != "127.0.0.1" || conexion.Database != "divorcios_pruebas_a" || conexion.Port == 5433)
                throw new InvalidOperationException("Las pruebas sólo admiten su PostgreSQL temporal aislado.");
            var opciones = new DbContextOptionsBuilder<DivorciosDbContext>().UseNpgsql(Conexion).UseSnakeCaseNamingConvention();
            if (interceptor is not null) opciones.AddInterceptors(interceptor);
            return new(opciones.Options);
        }

        public Task DisposeAsync()
        {
            if (nombre.StartsWith("divorcios-pruebas-a-", StringComparison.Ordinal))
            {
                // Sólo el contenedor temporal creado por esta instancia; ningún volumen persistente.
                try { Docker("stop", nombre); } catch (InvalidOperationException) { }
            }
            return Task.CompletedTask;
        }

        private static string Docker(params string[] argumentos)
        {
            var inicio = new ProcessStartInfo("docker") { RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
            foreach (var argumento in argumentos) inicio.ArgumentList.Add(argumento);
            using var proceso = Process.Start(inicio) ?? throw new InvalidOperationException("No se pudo iniciar Docker.");
            var salida = proceso.StandardOutput.ReadToEnd();
            var errores = proceso.StandardError.ReadToEnd();
            proceso.WaitForExit();
            if (proceso.ExitCode != 0) throw new InvalidOperationException("Falló Docker para la base temporal: " + errores);
            return salida;
        }
    }
}
