namespace Divorcios.Datos.Opciones
{
    public sealed class ReniecOpciones
    {
        public const string Seccion = "Reniec";
        public const int MaximoDiarioProveedor = 10;
        public bool Habilitado { get; set; }
        public string? Usuario { get; set; }
        public string? Clave { get; set; }
        public int? VigenciaCacheMinutos { get; set; }
        // Límite compartido de intentos API en una ventana móvil de 24 horas (instantes UTC).
        public int? LimiteDiario { get; set; } = MaximoDiarioProveedor;
        public int TiempoEsperaSegundos { get; set; } = 15;

        public bool EsValida => Habilitado
            && !string.IsNullOrWhiteSpace(Usuario)
            && !string.IsNullOrWhiteSpace(Clave)
            && (VigenciaCacheMinutos is null or > 0)
            && LimiteDiario is > 0 and <= MaximoDiarioProveedor
            && TiempoEsperaSegundos is > 0 and <= 60;
    }
}
