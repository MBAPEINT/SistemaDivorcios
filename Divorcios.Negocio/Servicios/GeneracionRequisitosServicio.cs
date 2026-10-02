using Divorcios.Datos.Interfaces;
using Divorcios.Dominio.Entidades;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Matrices;
using Divorcios.Negocio.Opciones;
using Divorcios.Negocio.Resultados;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Divorcios.Negocio.Servicios
{
    public sealed class GeneracionRequisitosServicio(IGeneracionRequisitosRepositorio repositorio,
        IOptions<MatrizRequisitosOpciones> opciones, IOptions<ArchivosPreregistroOpciones> archivos) : IGeneracionRequisitosServicio
    {
        public async Task<GeneracionRequisitosResultado> GenerarAsync(PreregistroVersion version, CancellationToken cancellationToken)
        {
            if (!opciones.Value.HabilitarGeneracionParcial)
                return new("PENDIENTE_CONFIGURACION", null, ["MATRIZ_RESPUESTA_REQUISITO", "TIPOS_Y_CANTIDAD_DOCUMENTAL"], []);
            var determinacion = MatrizRequisitosPreregistro.Determinar(version);
            var catalogos = await repositorio.ObtenerCatalogosAsync(determinacion.Requisitos.Select(x => x.Codigo).ToArray(), cancellationToken);
            var tipos = await repositorio.ObtenerTiposAsync(determinacion.Requisitos.SelectMany(x => x.TiposDocumentoCodigos).Distinct().ToArray(), cancellationToken);
            List<string> pendientes = determinacion.Pendientes.ToList();
            List<(PreregistroRequisito Requisito, DefinicionRequisito Definicion)> generados = [];
            foreach (var definicion in determinacion.Requisitos)
            {
                var catalogo = catalogos.SingleOrDefault(x => x.Codigo == definicion.Codigo);
                var mappings = archivos.Value.TiposPorRequisito.Where(x => x.RequisitoCodigo == definicion.Codigo).ToArray();
                if (catalogo is null || definicion.TiposDocumentoCodigos.Any(x => !tipos.Any(t => t.Codigo == x))
                    || mappings.Length != 1 || !mappings[0].Confirmada
                    || !mappings[0].TiposDocumentoCodigos.ToHashSet(StringComparer.Ordinal).SetEquals(definicion.TiposDocumentoCodigos))
                {
                    pendientes.Add("CONFIGURACION_REQUISITO_" + definicion.Codigo);
                    continue;
                }
                generados.Add((new() { PreregistroVersionId = version.PreregistroVersionId, RequisitoCatalogoId = catalogo.RequisitoCatalogoId,
                    Aplica = true, Obligatorio = true, EstadoCodigo = "PENDIENTE", GeneradoEn = version.CreadoEn }, definicion));
            }
            await repositorio.AgregarAsync(generados.Select(x => x.Requisito).ToArray(), cancellationToken);
            return new(generados.Count == 0 ? "PENDIENTE_CONFIGURACION" : "GENERACION_PARCIAL", MatrizRequisitosPreregistro.Version,
                pendientes.Distinct(StringComparer.Ordinal).ToArray(), generados.Select(x => new RequisitoGenerado(
                    x.Requisito.PreregistroRequisitoId.ToString(CultureInfo.InvariantCulture), x.Definicion)).ToArray());
        }
    }
}
