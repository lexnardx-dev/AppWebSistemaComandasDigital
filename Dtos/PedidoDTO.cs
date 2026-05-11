using System.ComponentModel.DataAnnotations;
using AppWebSistemaComandasDigital.Models;

namespace AppWebSistemaComandasDigital.Dtos
{
    // ── Entrada ──────────────────────────────────────────────────────
    public class PedidoCreateDTO
    {
        [Required]
        public int MesaId { get; set; }

        [MaxLength(300)]
        public string? Observaciones { get; set; }

        [Required, MinLength(1)]
        public List<DetallePedidoCreateDTO> Detalles { get; set; } = [];
    }

    // Estado como string para evitar problemas de deserialización de enums
    public class PedidoUpdateEstadoDTO
    {
        [Required]
        public string Estado { get; set; } = string.Empty;

        // Convierte el string al enum — lanza excepción si es inválido
        public EstadoPedido EstadoEnum =>
            Enum.Parse<EstadoPedido>(Estado, ignoreCase: true);
    }

    // ── Salida ───────────────────────────────────────────────────────
    public class PedidoDTO
    {
        public int Id { get; set; }
        public int MesaId { get; set; }
        public string MesaNumero { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<DetallePedidoDTO> Detalles { get; set; } = [];
    }
}
