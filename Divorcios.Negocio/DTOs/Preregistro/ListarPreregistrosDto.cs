using System.ComponentModel.DataAnnotations;

namespace Divorcios.Negocio.DTOs.Preregistro
{
    public sealed class ListarPreregistrosDto
    {
        [RegularExpression("^(BORRADOR|ENVIADO|OBSERVADO|APROBADO|CANCELADO)$")]
        public string? EstadoCodigo { get; set; }

        [Range(1, int.MaxValue)]
        public int Pagina { get; set; } = 1;

        [Range(1, 100)]
        public int TamanoPagina { get; set; } = 20;
    }
}
