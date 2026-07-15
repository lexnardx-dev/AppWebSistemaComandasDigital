using AppWebSistemaComandasDigital.Repositories;
using System.Security.Claims;

namespace AppWebSistemaComandasDigital.Services
{
    public sealed class UsuarioActualDTO
    {
        public int Id { get; init; }
        public string Nombre { get; init; } = "Usuario";
        public string Rol { get; init; } = string.Empty;
        public string? ImagenUrl { get; init; }
        public string Inicial => string.IsNullOrWhiteSpace(Nombre)
            ? "U"
            : Nombre.Trim()[0].ToString().ToUpperInvariant();
    }

    public class UsuarioActualService(
        IHttpContextAccessor httpContextAccessor,
        IUsuarioRepository usuarioRepository)
    {
        public async Task<UsuarioActualDTO?> ObtenerAsync()
        {
            var user = httpContextAccessor.HttpContext?.User;
            var idClaim = user?.FindFirstValue("usuario_id");

            if (!int.TryParse(idClaim, out var usuarioId))
                return null;

            var usuario = await usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario is null)
                return null;

            return new UsuarioActualDTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Rol = usuario.Rol?.Nombre ?? user?.FindFirstValue("rol_nombre") ?? string.Empty,
                ImagenUrl = usuario.ImagenUrl
            };
        }
    }
}
