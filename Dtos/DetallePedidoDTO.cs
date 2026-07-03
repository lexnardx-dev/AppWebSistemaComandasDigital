using System.ComponentModel.DataAnnotations;

namespace AppWebSistemaComandasDigital.Dtos
{
    public class DetallePedidoCreateDTO
    {
        [Required]
        public int PlatoId { get; set; }

        [Range(1, 99)]
        public int Cantidad { get; set; }

        [MaxLength(200)]
        public string? Notas { get; set; }
    }

    public class DetallePedidoDTO
    {
        public int Id { get; set; }
        public int PlatoId { get; set; }
        public string PlatoNombre { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string? Notas { get; set; }
    }
}