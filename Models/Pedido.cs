using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWebSistemaComandasDigital.Models
{
    public enum EstadoPedido
    {
        Pendiente = 0,
        EnCocina = 1,
        Listo = 2,
        Entregado = 3,
        Cancelado = 4
    }

    public class Pedido
    {
        public int Id { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

        [MaxLength(300)]
        public string? Observaciones { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        // FKs
        public int MesaId { get; set; }
        public int UsuarioId { get; set; }

        // Navegación
        public Mesa Mesa { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public ICollection<DetallePedido> Detalles { get; set; } = [];
    }
}
