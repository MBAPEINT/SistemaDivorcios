using Divorcios.Datos.Contexto;
using Divorcios.Datos.Interfaces;
using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Divorcios.Datos.Repositorios
{
    public sealed class GeneracionRequisitosRepositorio(DivorciosDbContext contexto) : IGeneracionRequisitosRepositorio
    {
        public async Task<IReadOnlyList<RequisitoCatalogo>> ObtenerCatalogosAsync(IReadOnlyList<string> codigos, CancellationToken cancellationToken)
            => await contexto.RequisitosCatalogo.AsNoTracking().Where(x => codigos.Contains(x.Codigo) && x.Activo).ToListAsync(cancellationToken);
        public async Task<IReadOnlyList<TipoDocumento>> ObtenerTiposAsync(IReadOnlyList<string> codigos, CancellationToken cancellationToken)
            => await contexto.TiposDocumento.AsNoTracking().Where(x => codigos.Contains(x.Codigo) && x.Activo
                && (x.OrigenCodigo == "CIUDADANO" || x.OrigenCodigo == "AMBOS")).ToListAsync(cancellationToken);
        public async Task AgregarAsync(IReadOnlyList<PreregistroRequisito> requisitos, CancellationToken cancellationToken)
        {
            contexto.PreregistrosRequisitos.AddRange(requisitos);
            await contexto.SaveChangesAsync(cancellationToken);
        }
    }
}
