using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Helpers;
using AppWebSistemaComandasDigital.Models;
using AppWebSistemaComandasDigital.Options;
using AppWebSistemaComandasDigital.Repositories;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AppWebSistemaComandasDigital.Controllers
{
    [Authorize]
    public class PerfilController(
        IUsuarioRepository usuarioRepository,
        ISupabaseStorageService storage,
        IOptions<SupabaseStorageOptions> storageOptions,
        JwtHelper jwtHelper,
        IConfiguration configuration) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var usuario = await ObtenerUsuarioActualAsync();
            return usuario is null ? Unauthorized() : View(CrearVista(usuario));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Actualizar([Bind(Prefix = "Editar")] EditarPerfilDTO model)
        {
            var usuario = await ObtenerUsuarioActualAsync();
            if (usuario is null) return Unauthorized();

            if (!ModelState.IsValid)
                return View(nameof(Index), CrearVista(usuario, model));

            if (model.ImagenArchivo is not null)
            {
                try
                {
                    model.ImagenUrl = await storage.SubirImagenAsync(
                        model.ImagenArchivo, storageOptions.Value.Buckets.Usuarios, HttpContext.RequestAborted);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Editar.ImagenArchivo", ex.Message);
                    return View(nameof(Index), CrearVista(usuario, model));
                }
            }

            usuario.Nombre = model.Nombre.Trim();
            usuario.ImagenUrl = model.ImagenUrl?.Trim();
            await usuarioRepository.UpdateAsync(usuario);
            RenovarCookie(usuario);

            TempData["Exito"] = "Perfil actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword([Bind(Prefix = "Password")] CambiarPasswordDTO model)
        {
            var usuario = await ObtenerUsuarioActualAsync();
            if (usuario is null) return Unauthorized();

            if (!ModelState.IsValid)
                return View(nameof(Index), CrearVista(usuario, password: model));

            if (AuthService.HashPassword(model.PasswordActual) != usuario.PasswordHash)
            {
                ModelState.AddModelError("Password.PasswordActual", "La contraseña actual no es correcta.");
                return View(nameof(Index), CrearVista(usuario, password: model));
            }

            usuario.PasswordHash = AuthService.HashPassword(model.NuevaPassword);
            await usuarioRepository.UpdateAsync(usuario);
            TempData["Exito"] = "Contraseña actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private PerfilViewDTO CrearVista(
            Usuario usuario,
            EditarPerfilDTO? editar = null,
            CambiarPasswordDTO? password = null) => new()
        {
            Usuario = new PerfilDTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol?.Nombre ?? string.Empty,
                Activo = usuario.Activo,
                FechaCreacion = usuario.FechaCreacion,
                ImagenUrl = usuario.ImagenUrl
            },
            Editar = editar ?? new EditarPerfilDTO
            {
                Nombre = usuario.Nombre,
                ImagenUrl = usuario.ImagenUrl
            },
            Password = password ?? new CambiarPasswordDTO()
        };

        private async Task<Usuario?> ObtenerUsuarioActualAsync()
        {
            var idClaim = User.FindFirstValue("usuario_id");
            return int.TryParse(idClaim, out var usuarioId)
                ? await usuarioRepository.GetByIdAsync(usuarioId)
                : null;
        }

        private void RenovarCookie(Usuario usuario)
        {
            var token = jwtHelper.GenerarToken(usuario);
            var minutos = double.Parse(configuration["Jwt:ExpiresInMinutes"]!);
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(minutos)
            });
        }
    }
}
