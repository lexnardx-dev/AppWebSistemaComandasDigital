using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWebSistemaComandasDigital.Models
{
    public class DetallePedido
    {
        public int Id { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        [MaxLength(200)]
        public string? Notas { get; set; }

        // FKs
        public int PedidoId { get; set; }
        public int PlatoId { get; set; }

        // Navegación
        public Pedido Pedido { get; set; } = null!;
        public Plato Plato { get; set; } = null!;
    }
}
