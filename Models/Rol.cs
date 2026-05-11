using System.ComponentModel.DataAnnotations;

namespace AppWebSistemaComandasDigital.Models
{
    public class Rol
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        // Navegación
        public ICollection<Usuario> Usuarios { get; set; } = [];
    }
}
