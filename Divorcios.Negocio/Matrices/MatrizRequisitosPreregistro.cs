using Divorcios.Dominio.Entidades;

namespace Divorcios.Negocio.Matrices
{
    public sealed record DefinicionRequisito(string Codigo, string Nombre, string TitularesCodigo,
        int? CantidadTitulares, string CoberturaEsperada, IReadOnlyList<string> TiposDocumentoCodigos,
        IReadOnlyList<string> Fuentes);
    public sealed record DeterminacionMatriz(IReadOnlyList<DefinicionRequisito> Requisitos,
        IReadOnlyList<string> Pendientes);

    // Códigos internos, no códigos oficiales. No es un motor de admisión o aprobación jurídica.
    public static class MatrizRequisitosPreregistro
    {
        public const string Version = "FUENTES_2026_10_01_PARCIAL_1";
        public const string Ley = "https://www.leyes.congreso.gob.pe/Documentos/Leyes/29227.pdf";
        public const string Reglamento = "https://www.muniate.gob.pe/wp-content/uploads/2026/02/DECRETO-SUPREMO-N%C2%B0-009-2008-JUS.pdf";
        public const string Municipal = "https://muniporvenir.gob.pe/servicios/tramite-de-divorcio/";
        public static DeterminacionMatriz Determinar(PreregistroVersion v)
        {
            List<DefinicionRequisito> requisitos = [];
            // Sin estos puntos cerrados nunca se acredita una matriz completa, aun con todos los archivos.
            List<string> pendientes = ["IDENTIFICACION_COPIAS_O_EXHIBICION", "PATRIMONIO_CLASIFICACION_PENDIENTE",
                "ALCANCE_HIJOS_COMUNES_O_DE_CADA_CONYUGE", "ETAPA_ACREDITACION_PAGO", "MATRIZ_COMPLETA_PENDIENTE_VALIDACION"];
            void Agregar(string codigo, string nombre, string titulares, int? cantidad, string cobertura, string[] tipos, params string[] fuentes)
                => requisitos.Add(new(codigo, nombre, titulares, cantidad, cobertura, tipos, fuentes));
            Agregar("PRE_SOLICITUD", "Solicitud de separación convencional", "AMBOS", 2,
                "Solicitud conjunta con contenido exigido y firmas/huellas de ambos; formulario municipal de referencia.",
                ["SOL_SEPARACION_CONVENCIONAL"], Ley + " (art. 5)", Reglamento + " (art. 6)", Municipal + " (formulario 1)");
            Agregar("PRE_MATRIMONIO", "Documentación matrimonial", "AMBOS", 2,
                "Acta o partida del vínculo matrimonial; calidad y emisión se comprueban al revisar.",
                ["ACTA_PARTIDA_MATRIMONIO"], Ley + " (art. 5.b)", Reglamento + " (art. 6.b)", "https://cdn.www.gob.pe/uploads/document/file/4999934/TUPA-MDEP.pdf?v=1692228069 (p.175; requisito 3)");
            if (v.CantidadHijosMenores > 0)
            {
                Agregar("PRE_NACIMIENTOS_MENORES", "Nacimiento de los hijos menores", "CADA_MENOR", v.CantidadHijosMenores,
                    "Cubrir cada menor; cantidad de titulares no equivale a cantidad de PDFs.", ["ACTA_PARTIDA_NACIMIENTO"],
                    Ley + " (art. 5.d)", Reglamento + " (art. 6.d)", "https://cdn.www.gob.pe/uploads/document/file/4999934/TUPA-MDEP.pdf?v=1692228069 (p.175; requisito 6)");
                Agregar("PRE_REGIMEN_MENORES", "Acuerdos o decisiones sobre los hijos menores", "CADA_MENOR", v.CantidadHijosMenores,
                    "Sentencia firme o conciliación que cubra patria potestad, alimentos, tenencia y visitas pertinentes; no exigir ambas alternativas.",
                    ["SENTENCIA_REGIMEN_MENORES", "ACTA_CONCILIACION_MENORES"], Ley + " (art. 4.a)", Reglamento + " (art. 6.e)");
                pendientes.Add("COBERTURA_INDIVIDUAL_MENORES_PENDIENTE_REVISION");
            }
            else
                Agregar("PRE_DJ_SIN_MENORES", "Declaración de no tener hijos menores", "AMBOS", 2,
                    "Declaración conjunta de ambos según el formulario aplicable; no duplicar el ítem repetido del TUPA.",
                    ["DJ_SIN_HIJOS_MENORES"], Reglamento + " (art. 6.c)", Municipal + " (formulario 3)");

            if (v.CantidadHijosMayores == 0 || v.TieneHijosMayoresSituacionEspecial == false)
                Agregar("PRE_DJ_SIN_MAYORES_ESPECIAL", "Declaración sin hijos mayores en el supuesto especial", "AMBOS", 2,
                    "Declaración pertinente firmada por ambos; mayores ordinarios no activan el supuesto especial.",
                    ["DJ_SIN_MAYORES_SUPUESTO"], Reglamento + " (art. 6.c)", Municipal + " (formulario 4)");
            else if (v.TieneHijosMayoresSituacionEspecial is null)
                pendientes.Add("RESPUESTA_PENDIENTE_SITUACION_ESPECIAL_MAYORES");
            else
            {
                Agregar("PRE_NACIMIENTOS_MAYORES_ESPECIAL", "Nacimiento de los mayores comprendidos en el supuesto especial", "CADA_MAYOR_SUPUESTO", null,
                    "Sólo mayores comprendidos; la cantidad total de mayores no identifica cuántos están en el supuesto.",
                    ["ACTA_PARTIDA_NACIMIENTO"], Reglamento + " (art. 6.d)", "https://cdn.www.gob.pe/uploads/document/file/4999934/TUPA-MDEP.pdf?v=1692228069 (p.175; requisito 6)");
                pendientes.Add("MAYORES_SUPUESTO_Y_DOCUMENTACION_REVISION_JURIDICA");
                // No activar curatela/interdicción ni patria potestad de menores por semejanza de términos.
            }
            void Representacion(string posicion, bool? respuesta)
            {
                if (respuesta is null) pendientes.Add("RESPUESTA_PENDIENTE_REPRESENTACION_" + posicion);
                else if (respuesta == true)
                    Agregar("PRE_PODER_" + posicion, "Poder para representar al cónyuge " + posicion, posicion, 1,
                        "Escritura pública con facultades específicas e inscripción; revisar cobertura, vigencia y facultades. No crea Representacion.",
                        ["PODER_ESPECIAL_INSCRITO"], Reglamento + " (art. 15)");
            }
            Representacion("A", v.RequiereRepresentacionA); Representacion("B", v.RequiereRepresentacionB);
            if (!v.MatrimonioEnPorvenir && v.UltimoDomicilioConyugalPorvenir)
                Agregar("PRE_DOMICILIO_CONYUGAL", "Declaración de último domicilio conyugal", "AMBOS", 2,
                    "Declaración de ambos cuando se invoca esta vía territorial; no reemplazar con domicilio actual de RENIEC.",
                    ["DJ_ULTIMO_DOMICILIO_CONYUGAL"], Ley + " (art. 3)", Reglamento + " (art. 6.j)", Municipal + " (formulario 2)");
            else if (v.MatrimonioEnPorvenir)
                pendientes.Add("ALCANCE_DECLARACION_DOMICILIO_POR_MATRIMONIO");
            else pendientes.Add("COMPETENCIA_TERRITORIAL_PENDIENTE_REVISION");
            // TieneBienes/TieneAcuerdoBienes no prueban sociedad de gananciales ni escritura inscrita.
            return new(requisitos, pendientes);
        }
    }
}
