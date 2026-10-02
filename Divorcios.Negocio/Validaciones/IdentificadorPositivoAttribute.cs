using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Divorcios.Negocio.Validaciones
{
    public sealed class IdentificadorPositivoAttribute : ValidationAttribute
    {
        public IdentificadorPositivoAttribute() : base("El identificador debe ser una cadena decimal de un entero positivo dentro del rango Int64.") { }

        public bool PermitirNull { get; set; }

        public override bool IsValid(object? value) => value is null ? PermitirNull : value is string cadena
            && cadena.Length is > 0 and <= 19 && cadena[0] is >= '1' and <= '9'
            && long.TryParse(cadena, NumberStyles.None, CultureInfo.InvariantCulture, out var id) && id > 0;

        public static long Leer(string valor)
        {
            if (!new IdentificadorPositivoAttribute().IsValid(valor))
                throw new ValidationException("El identificador debe ser una cadena decimal de un entero positivo dentro del rango Int64.");
            return long.Parse(valor, CultureInfo.InvariantCulture);
        }
    }
}
