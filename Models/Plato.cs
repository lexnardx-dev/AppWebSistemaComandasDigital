using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWebSistemaComandasDigital.Models
{
    public class Plato
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        public bool Disponible { get; set; } = true;

        [MaxLength(250)]
        public string? ImagenUrl { get; set; }

        // FK
        public int CategoriaId { get; set; }

        // Navegación
        public Categoria Categoria { get; set; } = null!;
        public ICollection<DetallePedido> Detalles { get; set; } = [];
    }
}
