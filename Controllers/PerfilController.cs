using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Repositories;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppWebSistemaComandasDigital.Controllers
{
    [Authorize]
    public class PerfilController(IUsuarioRepository usuarioRepository) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var usuario = await ObtenerUsuarioActualAsync();
            if (usuario is null) return Unauthorized();

            return View(new PerfilViewDTO
            {
                Usuario = new PerfilDTO
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    Rol = usuario.Rol?.Nombre ?? "",
                    Activo = usuario.Activo,
                    FechaCreacion = usuario.FechaCreacion
                }
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(PerfilViewDTO model)
        {
            var usuario = await ObtenerUsuarioActualAsync();
            if (usuario is null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                model.Usuario = new PerfilDTO
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    Rol = usuario.Rol?.Nombre ?? "",
                    Activo = usuario.Activo,
                    FechaCreacion = usuario.FechaCreacion
                };
                return View(nameof(Index), model);
            }

            if (AuthService.HashPassword(model.Password.PasswordActual) != usuario.PasswordHash)
            {
                TempData["Error"] = "La contrasena actual no es correcta.";
                return RedirectToAction(nameof(Index));
            }

            usuario.PasswordHash = AuthService.HashPassword(model.Password.NuevaPassword);
            await usuarioRepository.UpdateAsync(usuario);

            TempData["Exito"] = "Contrasena actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<AppWebSistemaComandasDigital.Models.Usuario?> ObtenerUsuarioActualAsync()
        {
            var idClaim = User.FindFirstValue("usuario_id");
            return int.TryParse(idClaim, out var usuarioId)
                ? await usuarioRepository.GetByIdAsync(usuarioId)
                : null;
        }
    }
}
