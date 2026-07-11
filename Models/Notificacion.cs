using System.ComponentModel.DataAnnotations;

namespace AppWebSistemaComandasDigital.Models
{
    public enum PrioridadNotificacion { Normal, Importante, Urgente }

    public static class TiposNotificacion
    {
        public const string Pedido = "Pedido";
        public const string EstadoPedido = "EstadoPedido";
        public const string Mesa = "Mesa";
        public const string Sistema = "Sistema";
    }

    public class Notificacion
    {
        public int Id { get; set; }

        [Required, MaxLength(40)]
        public string Tipo { get; set; } = TiposNotificacion.Sistema;

        [Required, MaxLength(140)]
        public string Titulo { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Mensaje { get; set; } = string.Empty;

        public PrioridadNotificacion Prioridad { get; set; }
        public bool Leida { get; set; }
        public int? UsuarioDestinoId { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [MaxLength(300)]
        public string? EnlaceRelacionado { get; set; }

        public Usuario? UsuarioDestino { get; set; }
    }
}
