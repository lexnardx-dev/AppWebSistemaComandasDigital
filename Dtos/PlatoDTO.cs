using System.ComponentModel.DataAnnotations;

namespace AppWebSistemaComandasDigital.Dtos
{
    // ── Entrada ──────────────────────────────────────────────────────
    public class PlatoCreateDTO
    {
        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Descripcion { get; set; }

        [Range(0.50, 9999.50, ErrorMessage = "El precio debe estar entre 0.50 y 9999.50.")]
        public decimal Precio { get; set; }

        public bool Disponible { get; set; } = true;

        [MaxLength(250)]
        public string? ImagenUrl { get; set; }

        [Required]
        public int CategoriaId { get; set; }
    }

    public class PlatoUpdateDTO : PlatoCreateDTO { }

    // ── Salida ───────────────────────────────────────────────────────
    public class PlatoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public bool Disponible { get; set; }
        public string? ImagenUrl { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
    }
}
