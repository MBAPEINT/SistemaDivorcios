using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Divorcios.Negocio.DTOs.Catalogos
{
    public sealed class CrearTipoDocumentoDto
    {
        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(40, ErrorMessage = "El código no puede tener más de 40 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(160, ErrorMessage = "El nombre no puede tener más de 160 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El origen es obligatorio")]
        [StringLength(20)]
        [RegularExpression("^(CIUDADANO|MUNICIPALIDAD|AMBOS)$", ErrorMessage = "El origen debe ser CIUDADANO, MUNICIPALIDAD o AMBOS")]
        public string OrigenCodigo { get; set; } = string.Empty;
    }
}
