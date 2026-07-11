using System.ComponentModel.DataAnnotations;

namespace AppWebSistemaComandasDigital.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? ImagenUrl { get; set; }

        // FK
        public int RolId { get; set; }

        // Navegación
        public Rol Rol { get; set; } = null!;
        public ICollection<Pedido> Pedidos { get; set; } = [];
        public ICollection<Notificacion> Notificaciones { get; set; } = [];
    }
}
