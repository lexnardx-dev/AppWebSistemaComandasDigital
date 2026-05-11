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
    }

    public class CambiarPasswordDTO
    {
        [Required(ErrorMessage = "Ingresa tu contrasena actual.")]
        public string PasswordActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresa la nueva contrasena.")]
        [MinLength(8, ErrorMessage = "La nueva contrasena debe tener minimo 8 caracteres.")]
        public string NuevaPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma la nueva contrasena.")]
        [Compare(nameof(NuevaPassword), ErrorMessage = "Las contrasenas no coinciden.")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }

    public class PerfilViewDTO
    {
        public PerfilDTO Usuario { get; set; } = new();
        public CambiarPasswordDTO Password { get; set; } = new();
    }
}
