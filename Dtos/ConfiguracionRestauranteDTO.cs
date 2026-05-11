using System.ComponentModel.DataAnnotations;

namespace AppWebSistemaComandasDigital.Dtos
{
    public class ConfiguracionRestauranteDTO
    {
        [Required(ErrorMessage = "El nombre del restaurante es obligatorio.")]
        [MaxLength(120)]
        public string NombreRestaurante { get; set; } = "Comandas Digital";

        [MaxLength(20)]
        public string? Ruc { get; set; }

        [MaxLength(180)]
        public string? Direccion { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [Required]
        [MaxLength(8)]
        public string Moneda { get; set; } = "S/";

        [Range(0, 30, ErrorMessage = "El IGV debe estar entre 0 y 30.")]
        public decimal IgvPorcentaje { get; set; } = 18m;

        [MaxLength(120)]
        public string? HorarioAtencion { get; set; }

        [MaxLength(250)]
        public string? LogoUrl { get; set; }

        [Required]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Usa un color hexadecimal valido. Ej: #F6B700.")]
        public string ColorMarca { get; set; } = "#F6B700";
    }
}
