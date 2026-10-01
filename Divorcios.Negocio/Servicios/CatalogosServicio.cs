using System.ComponentModel.DataAnnotations;
using Divorcios.Datos.Interfaces;
using Divorcios.Dominio.Entidades;
using Divorcios.Negocio.DTOs.Catalogos;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Excepciones;
using Divorcios.Datos.Excepciones;

namespace Divorcios.Negocio.Servicios
{
    public sealed class CatalogosServicio(ICatalogosRepositorio repositorio) : ICatalogosServicio
    {
        public async Task<IReadOnlyList<EstadoExpedienteDto>> ListarEstadosExpedienteAsync(
            bool? activo, CancellationToken cancellationToken)
        {
            var registros = await repositorio.ListarEstadosExpedienteAsync(activo, cancellationToken);
            return registros.Select(Mapear).ToArray();
        }

        public async Task<EstadoExpedienteDto?> ObtenerEstadoExpedienteAsync(
            int id, CancellationToken cancellationToken)
        {
            var registro = await repositorio.ObtenerEstadoExpedienteAsync(id, cancellationToken);
            return registro is null ? null : Mapear(registro);
        }

        private static EstadoExpedienteDto Mapear(EstadoExpediente registro)
        {
            return new EstadoExpedienteDto(
                registro.EstadoExpedienteId,
                registro.Codigo,
                registro.NombreCiudadano,
                registro.EtapaCodigo,
                registro.OrdenVisual,
                registro.EsFinal,
                registro.Activo);
        }

        public async Task<IReadOnlyList<TipoDocumentoDto>> ListarTiposDocumentoAsync(
            bool? activo, CancellationToken cancellationToken)
        {
            var registros = await repositorio.ListarTiposDocumentoAsync(activo, cancellationToken);
            return registros.Select(Mapear).ToArray();
        }

        public async Task<TipoDocumentoDto?> ObtenerTipoDocumentoAsync(
            int id, CancellationToken cancellationToken)
        {
            var registro = await repositorio.ObtenerTipoDocumentoAsync(id, cancellationToken);
            return registro is null ? null : Mapear(registro);
        }

        public async Task<TipoDocumentoDto> CrearTipoDocumentoAsync(
            CrearTipoDocumentoDto datos, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(datos);

            Validator.ValidateObject(
                datos,
                new ValidationContext(datos),
                validateAllProperties: true);

            var codigo = datos.Codigo.Trim();
            var existeCodigo = await repositorio.ExisteCodigoTipoDocumentoAsync(codigo, cancellationToken);
            if (existeCodigo)
            {
                throw new ConflictoNegocioException("Ya existe un tipo de documento con ese código.");
            }

            var tipoDocumento = new TipoDocumento
            {
                Codigo = codigo,
                Nombre = datos.Nombre.Trim(),
                OrigenCodigo = datos.OrigenCodigo.Trim(),
                Activo = true
            };

            try
            {
                var guardado = await repositorio.CrearTipoDocumentoAsync(
                    tipoDocumento, cancellationToken);

                return Mapear(guardado);
            }
            catch (CodigoDuplicadoPersistenciaException excepcion)
            {
                throw new ConflictoNegocioException(
                    "Ya existe un tipo de documento con ese código.",
                    excepcion);
            }
        }

        private static TipoDocumentoDto Mapear(TipoDocumento registro)
        {
            return new TipoDocumentoDto(
                registro.TipoDocumentoId,
                registro.Codigo,
                registro.Nombre,
                registro.OrigenCodigo,
                registro.Activo);
        }

        public async Task<IReadOnlyList<RequisitoCatalogoDto>> ListarRequisitosAsync(
            bool? activo, CancellationToken cancellationToken)
        {
            var registros = await repositorio.ListarRequisitosAsync(activo, cancellationToken);
            return registros.Select(Mapear).ToArray();
        }

        public async Task<RequisitoCatalogoDto?> ObtenerRequisitoCatalogoAsync(
            int id, CancellationToken cancellationToken)
        {
            var registro = await repositorio.ObtenerRequisitoCatalogoAsync(id, cancellationToken);
            return registro is null ? null : Mapear(registro);
        }

        private static RequisitoCatalogoDto Mapear(RequisitoCatalogo registro)
        {
            return new RequisitoCatalogoDto(
                registro.RequisitoCatalogoId,
                registro.Codigo,
                registro.Nombre,
                registro.Descripcion,
                registro.Activo);
        }

        public async Task<IReadOnlyList<DestinoOficioDto>> ListarDestinosOficioAsync(
            bool? activo, CancellationToken cancellationToken)
        {
            var registros = await repositorio.ListarDestinosOficioAsync(activo, cancellationToken);
            return registros.Select(Mapear).ToArray();
        }

        public async Task<DestinoOficioDto?> ObtenerDestinoOficioAsync(
            int id, CancellationToken cancellationToken)
        {
            var registro = await repositorio.ObtenerDestinoOficioAsync(id, cancellationToken);
            return registro is null ? null : Mapear(registro);
        }

        private static DestinoOficioDto Mapear(DestinoOficio registro)
        {
            return new DestinoOficioDto(
                registro.DestinoOficioId,
                registro.Codigo,
                registro.Nombre,
                registro.Activo);
        }

        public async Task<IReadOnlyList<ReglaPlazoDto>> ListarReglasPlazoAsync(
            bool? activo, CancellationToken cancellationToken)
        {
            var registros = await repositorio.ListarReglasPlazoAsync(activo, cancellationToken);
            return registros.Select(Mapear).ToArray();
        }

        public async Task<ReglaPlazoDto?> ObtenerReglaPlazoAsync(
            int id, CancellationToken cancellationToken)
        {
            var registro = await repositorio.ObtenerReglaPlazoAsync(id, cancellationToken);
            return registro is null ? null : Mapear(registro);
        }

        private static ReglaPlazoDto Mapear(ReglaPlazo registro)
        {
            return new ReglaPlazoDto(
                registro.ReglaPlazoId,
                registro.Codigo,
                registro.Nombre,
                registro.Descripcion,
                registro.Cantidad,
                registro.UnidadCodigo,
                registro.TipoDiaCodigo,
                registro.EventoInicioCodigo,
                registro.VigenteDesde,
                registro.VigenteHasta,
                registro.Fuente,
                registro.Activo);
        }

        public async Task<IReadOnlyList<DiaNoLaborableDto>> ListarDiasNoLaborablesAsync(
            bool? activo, CancellationToken cancellationToken)
        {
            var registros = await repositorio.ListarDiasNoLaborablesAsync(activo, cancellationToken);
            return registros.Select(Mapear).ToArray();
        }

        public async Task<DiaNoLaborableDto?> ObtenerDiaNoLaborableAsync(
            int id, CancellationToken cancellationToken)
        {
            var registro = await repositorio.ObtenerDiaNoLaborableAsync(id, cancellationToken);
            return registro is null ? null : Mapear(registro);
        }

        private static DiaNoLaborableDto Mapear(DiaNoLaborable registro)
        {
            return new DiaNoLaborableDto(
                registro.DiaNoLaborableId,
                registro.Fecha,
                registro.Nombre,
                registro.TipoCodigo,
                registro.AmbitoCodigo,
                registro.ExcluyeDiaHabil,
                registro.ExcluyeDiaOperativo,
                registro.Activo);
        }
    }
}
