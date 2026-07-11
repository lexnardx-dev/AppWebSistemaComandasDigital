using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWebSistemaComandasDigital.Models
{
    [Table("ConfiguracionRestaurante")]
    public class ConfiguracionRestaurante
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string NombreRestaurante { get; set; } = "Comandas Digital";

        [MaxLength(20)]
        public string? Ruc { get; set; }

        [MaxLength(180)]
        public string? Direccion { get; set; }

        [MaxLength(30)]
        public string? Telefono { get; set; }

        [Required, MaxLength(8)]
        public string Moneda { get; set; } = "S/";

        [Column(TypeName = "decimal(5,2)")]
        public decimal IgvPorcentaje { get; set; } = 18m;

        [MaxLength(120)]
        public string? HorarioAtencion { get; set; }

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [Required, MaxLength(7)]
        public string ColorMarca { get; set; } = "#F6B700";

        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    }
}
