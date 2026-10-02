using Divorcios.Datos.Resultados;

namespace Divorcios.Negocio.Servicios
{
    internal static class ReglasEdicionPreregistro
    {
        public static bool PermiteNuevaVersion(CabeceraPreregistro c)
        {
            var p = c.Registro;
            var e = p.Expediente;
            return (p.EstadoCodigo is "BORRADOR" or "OBSERVADO") && p.BloqueadoEn is null
                && e.PreregistroBloqueadoEn is null && e.OficializadoEn is null && e.NumeroExpediente is null && e.CerradoEn is null
                && !c.Revisiones.Any(x => x.ResultadoCodigo == "EN_REVISION" && x.FinalizadaEn is null);
        }
        public static bool EsVersionTrabajo(CabeceraPreregistro cabecera, long versionId)
        {
            var ultima = cabecera.Versiones.FirstOrDefault();
            return PermiteNuevaVersion(cabecera) && ultima?.VersionId == versionId
                && !cabecera.Revisiones.Any(x => x.VersionId == versionId)
                && cabecera.VersionEnviadaAuditoriaId != versionId.ToString(System.Globalization.CultureInfo.InvariantCulture)
                && !(cabecera.Registro.EnviadoEn >= ultima.CreadoEn);
        }
        public static bool EsVersionTrabajo(LecturaVersionesPreregistro lectura, long versionId)
            => EsVersionTrabajo(lectura.Cabecera, versionId)
                && !lectura.Eventos.Any(x => x.AccionCodigo == "PREREGISTRO_ENVIADO"
                    && x.RecursoId == versionId.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}
