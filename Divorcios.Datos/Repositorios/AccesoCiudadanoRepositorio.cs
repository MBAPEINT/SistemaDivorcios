using Divorcios.Datos.Contexto;
using Divorcios.Datos.Interfaces;
using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Divorcios.Datos.Repositorios
{
    public sealed class AccesoCiudadanoRepositorio(DivorciosDbContext contexto) : IAccesoCiudadanoRepositorio
    {
        public Task<CuentaCiudadana?> ObtenerPorDniAsync(string dni, CancellationToken cancellationToken)
            => contexto.CuentasCiudadanas.AsNoTracking().Include(x => x.Persona)
                .SingleOrDefaultAsync(x => x.Persona.Dni == dni, cancellationToken);

        public Task<CuentaCiudadana?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken)
            => contexto.CuentasCiudadanas.AsNoTracking().Include(x => x.Persona)
                .SingleOrDefaultAsync(x => x.CuentaCiudadanaId == id, cancellationToken);

        public async Task<T> EjecutarSerializadoAsync<T>(Func<Task<T>> operacion, CancellationToken cancellationToken)
        {
            await using var transaccion = await contexto.Database.BeginTransactionAsync(cancellationToken);
            await contexto.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(734021, 2)", cancellationToken);
            var resultado = await operacion();
            await transaccion.CommitAsync(cancellationToken);
            return resultado;
        }

        public async Task GuardarAsync(CuentaCiudadana cuenta, CancellationToken cancellationToken)
        {
            if (cuenta.CuentaCiudadanaId == 0) contexto.CuentasCiudadanas.Add(cuenta);
            else contexto.Entry(cuenta).State = EntityState.Modified;
            await contexto.SaveChangesAsync(cancellationToken);
            contexto.Entry(cuenta).State = EntityState.Detached;
        }
    }
}
