using System.Data;
using Divorcios.Datos.Contexto;
using Divorcios.Datos.Interfaces;
using Divorcios.Datos.Resultados;
using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Divorcios.Datos.Repositorios
{
    public sealed class ArchivosPreregistroRepositorio(DivorciosDbContext contexto) : IArchivosPreregistroRepositorio
    {
        public Task<PreregistroRequisito?> ObtenerRequisitoAsync(long versionId, long requisitoId, CancellationToken cancellationToken)
            => contexto.PreregistrosRequisitos.Include(x => x.RequisitoCatalogo).Include(x => x.DetallesRevision).Include(x => x.ArchivosPresentados)
                .ThenInclude(x => x.DocumentoVersion).ThenInclude(x => x.Documento)
                .SingleOrDefaultAsync(x => x.PreregistroVersionId == versionId && x.PreregistroRequisitoId == requisitoId, cancellationToken);

        public Task<TipoDocumento?> ObtenerTipoAsync(short tipoId, CancellationToken cancellationToken)
            => contexto.TiposDocumento.AsNoTracking().SingleOrDefaultAsync(x => x.TipoDocumentoId == tipoId, cancellationToken);

        public Task<Documento?> ObtenerDocumentoAsync(long expedienteId, long documentoId, CancellationToken cancellationToken)
            => contexto.Documentos.AsNoTracking().Include(x => x.TipoDocumento).Include(x => x.Versiones)
                .SingleOrDefaultAsync(x => x.ExpedienteId == expedienteId && x.DocumentoId == documentoId
                    && x.EtapaCodigo == "PRERREGISTRO", cancellationToken);

        public async Task<IReadOnlyList<DocumentoVersion>> ObtenerVersionesAsync(long expedienteId, IReadOnlyList<long> ids, CancellationToken cancellationToken)
            => await contexto.DocumentosVersiones.AsNoTracking().Include(x => x.Documento).ThenInclude(x => x.TipoDocumento)
                .Where(x => ids.Contains(x.DocumentoVersionId) && x.Documento.ExpedienteId == expedienteId
                    && x.Documento.EtapaCodigo == "PRERREGISTRO").ToListAsync(cancellationToken);

        public Task<DocumentoVersion?> ObtenerArchivoAsync(long preregistroId, long personaId, long documentoId, long versionId, CancellationToken cancellationToken)
            => contexto.DocumentosVersiones.AsNoTracking().Include(x => x.Documento)
                .SingleOrDefaultAsync(x => x.DocumentoVersionId == versionId && x.DocumentoId == documentoId
                    && x.Documento.EtapaCodigo == "PRERREGISTRO"
                    && x.Documento.Expediente.Preregistro!.PreregistroId == preregistroId
                    && x.Documento.Expediente.Conyuges.Any(c => c.PersonaId == personaId), cancellationToken);

        public async Task<PaginaDocumentosPreregistro> ListarAsync(long preregistroId, long personaId, short? tipoId,
            int pagina, int tamanoPagina, CancellationToken cancellationToken)
        {
            await using var transaccion = await contexto.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
            var consulta = contexto.Documentos.AsNoTracking().Where(x => x.EtapaCodigo == "PRERREGISTRO"
                && x.Expediente.Preregistro!.PreregistroId == preregistroId && x.Expediente.Conyuges.Any(c => c.PersonaId == personaId)
                && (tipoId == null || x.TipoDocumentoId == tipoId));
            var total = await consulta.LongCountAsync(cancellationToken);
            var desplazamiento = ((long)pagina - 1) * tamanoPagina;
            IReadOnlyList<Documento> items = desplazamiento > int.MaxValue ? [] : await consulta
                .OrderByDescending(x => x.CreadoEn).ThenByDescending(x => x.DocumentoId).Skip((int)desplazamiento).Take(tamanoPagina)
                .Include(x => x.TipoDocumento)
                .Include(x => x.PreregistroRequisito).ThenInclude(x => x!.PreregistroVersion)
                .Include(x => x.Versiones).ThenInclude(x => x.PresentacionesPreregistro).ThenInclude(x => x.PreregistroRequisito)
                    .ThenInclude(x => x.PreregistroVersion)
                .Include(x => x.Versiones).ThenInclude(x => x.Evaluaciones).ThenInclude(x => x.RevisionDetalle)
                    .ThenInclude(x => x.RevisionPreregistro).ThenInclude(x => x.PreregistroVersion)
                .AsSplitQuery().ToListAsync(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
            return new(items, total);
        }

        public async Task AgregarAsync(DocumentoVersion version, CancellationToken cancellationToken)
        {
            contexto.DocumentosVersiones.Add(version);
            await contexto.SaveChangesAsync(cancellationToken);
        }

        public async Task SeleccionarAsync(PreregistroRequisito requisito, IReadOnlyList<long> ids, CancellationToken cancellationToken)
        {
            var existentes = requisito.ArchivosPresentados.ToArray();
            foreach (var vinculo in existentes.Where(x => !ids.Contains(x.DocumentoVersionId)))
                contexto.PreregistrosRequisitosDocumentos.Remove(vinculo);
            foreach (var id in ids.Where(x => !existentes.Any(v => v.DocumentoVersionId == x)))
                contexto.PreregistrosRequisitosDocumentos.Add(new()
                {
                    PreregistroRequisitoId = requisito.PreregistroRequisitoId, DocumentoVersionId = id
                });
            requisito.EstadoCodigo = ids.Count == 0 ? requisito.Aplica ? "PENDIENTE" : "NO_APLICA" : "CARGADO";
            await contexto.SaveChangesAsync(cancellationToken);
        }

        public Task<bool> EstaClavePersistidaAsync(string clave, CancellationToken cancellationToken)
            => contexto.DocumentosVersiones.AsNoTracking().AnyAsync(x => x.AlmacenamientoClave == clave, cancellationToken);
    }
}
