using System.ComponentModel.DataAnnotations;

namespace AppWebSistemaComandasDigital.Dtos
{
    public class PerfilDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? ImagenUrl { get; set; }
    }

    public class EditarPerfilDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Url(ErrorMessage = "Ingresa una URL de imagen válida.")]
        [MaxLength(500)]
        public string? ImagenUrl { get; set; }

        public IFormFile? ImagenArchivo { get; set; }
    }

    public class CambiarPasswordDTO
    {
        [Required(ErrorMessage = "Ingresa tu contraseña actual.")]
        public string PasswordActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresa la nueva contraseña.")]
        [RegularExpression(
            "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^A-Za-z0-9]).{10,}$",
            ErrorMessage = "Usa al menos 10 caracteres, mayúscula, minúscula, número y símbolo.")]
        public string NuevaPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma la nueva contraseña.")]
        [Compare(nameof(NuevaPassword), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }

    public class PerfilViewDTO
    {
        public PerfilDTO Usuario { get; set; } = new();
        public EditarPerfilDTO Editar { get; set; } = new();
        public CambiarPasswordDTO Password { get; set; } = new();
    }
}
