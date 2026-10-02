using Divorcios.Negocio.DTOs.Preregistro;
using Divorcios.Negocio.Interfaces;
using Divorcios.Negocio.Opciones;
using Microsoft.Extensions.Options;

namespace Divorcios.Negocio.Servicios
{
    public sealed class InformacionPreregistroServicio(IOptions<InformacionPreregistroOpciones> opciones) : IInformacionPreregistroServicio
    {
        public InformacionPreregistroDto Obtener()
        {
            var configuracion = opciones.Value;
            var formatos = configuracion.Formatos.Where(x => !string.IsNullOrWhiteSpace(x.Codigo)
                && !string.IsNullOrWhiteSpace(x.Nombre) && Uri.TryCreate(x.Url, UriKind.Absolute, out var uri)
                && uri.Scheme == Uri.UriSchemeHttps).ToArray();
            var mime = configuracion.MimePermitidos.Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLowerInvariant()).Distinct().ToArray();
            var politica = configuracion.MaximoBytes is > 0 && mime.Length > 0
                ? new PoliticaArchivosDto(mime, configuracion.MaximoBytes.Value) : null;
            var pendientes = new List<string>();
            if (formatos.Length == 0) pendientes.Add("FORMATOS_OFICINA");
            if (politica is null) pendientes.Add("POLITICA_ARCHIVOS");
            return new(
                "El prerregistro permite presentar información y documentos para su revisión previa al ingreso formal del expediente.",
                ["Identificar a los dos cónyuges y al iniciador.",
                 "Completar la encuesta y presentar los archivos que correspondan.",
                 "Enviar a revisión y atender las observaciones.",
                 "Continuar con la presentación presencial cuando exista conformidad previa."],
                ["La conformidad digital no es una resolución de separación ni de divorcio.",
                 "El prerregistro digital no inicia los plazos del expediente formal.",
                 "Los formularios publicados son referencias; presente únicamente los que correspondan a su situación.",
                 "Subir archivos no acredita que los documentos estén completos o conformes."],
                formatos, politica, pendientes);
        }
    }
}
