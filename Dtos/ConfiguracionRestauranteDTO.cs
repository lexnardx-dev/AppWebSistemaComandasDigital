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

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        public IFormFile? LogoArchivo { get; set; }

        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    }
}
