using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppWebSistemaComandasDigital.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }

        public bool Activa { get; set; } = true;

        // Navegación
        public ICollection<Plato> Platos { get; set; } = [];
    }
}
