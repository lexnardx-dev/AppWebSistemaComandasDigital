using System.ComponentModel.DataAnnotations;

namespace AppWebSistemaComandasDigital.Models
{
    public enum EstadoMesa
    {
        Libre = 0,
        Ocupada = 1,
        Reservada = 2
    }

    public class Mesa
    {
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Numero { get; set; } = string.Empty;

        public int Capacidad { get; set; }

        public EstadoMesa Estado { get; set; } = EstadoMesa.Libre;

        // Navegación
        public ICollection<Pedido> Pedidos { get; set; } = [];
    }
}
