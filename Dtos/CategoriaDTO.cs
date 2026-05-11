using System.ComponentModel.DataAnnotations;

namespace AppWebSistemaComandasDigital.Dtos
{
    public class CategoriaCreateDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(80, ErrorMessage = "Máximo 80 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }
    }

    public class CategoriaUpdateDTO : CategoriaCreateDTO
    {
        public bool Activa { get; set; } = true;
    }

    public class CategoriaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activa { get; set; }
    }
}
