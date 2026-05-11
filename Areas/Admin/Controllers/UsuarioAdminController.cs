using AppWebSistemaComandasDigital.Data;
using AppWebSistemaComandasDigital.Models;
using AppWebSistemaComandasDigital.Repositories;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AppWebSistemaComandasDigital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "SoloAdmin")]
    public class UsuarioAdminController(
        IUsuarioRepository usuarioRepository,
        ApplicationDbContext db) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var usuarios = await usuarioRepository.GetAllAsync();
            return View(usuarios);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarRolesAsync();
            return View(new UsuarioCreateVM());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                await CargarRolesAsync();
                return View(model);
            }

            if (await usuarioRepository.ExisteEmailAsync(model.Email.ToLower()))
            {
                ModelState.AddModelError(nameof(model.Email), "Ya existe un usuario con ese correo.");
                await CargarRolesAsync();
                return View(model);
            }

            var rol = await db.RolesComandas.FindAsync(model.RolId);
            if (rol is null)
            {
                ModelState.AddModelError(string.Empty, "Rol inválido.");
                await CargarRolesAsync();
                return View(model);
            }

            await usuarioRepository.CreateAsync(new Usuario
            {
                Nombre        = model.Nombre.Trim(),
                Email         = model.Email.Trim().ToLower(),
                PasswordHash  = AuthService.HashPassword(model.Password),
                RolId         = model.RolId,
                Activo        = true,
                FechaCreacion = DateTime.UtcNow
            });

            TempData["Exito"] = $"Usuario '{model.Nombre}' creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            var usuario = await db.Usuarios.FindAsync(id);
            if (usuario is null) return NotFound();

            // Proteger al admin actual
            var emailActual = User.FindFirst("email")?.Value;
            if (usuario.Email == emailActual)
            {
                TempData["Error"] = "No puedes desactivar tu propia cuenta.";
                return RedirectToAction(nameof(Index));
            }

            usuario.Activo = !usuario.Activo;
            await db.SaveChangesAsync();

            TempData["Exito"] = usuario.Activo
                ? $"Usuario '{usuario.Nombre}' activado."
                : $"Usuario '{usuario.Nombre}' desactivado.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id, string nuevaPassword)
        {
            if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 8)
            {
                TempData["Error"] = "La contraseña debe tener mínimo 8 caracteres.";
                return RedirectToAction(nameof(Index));
            }

            var usuario = await db.Usuarios.FindAsync(id);
            if (usuario is null) return NotFound();

            usuario.PasswordHash = AuthService.HashPassword(nuevaPassword);
            await db.SaveChangesAsync();

            TempData["Exito"] = $"Contraseña de '{usuario.Nombre}' restablecida.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var emailActual = User.FindFirst("email")?.Value;
            var usuario     = await db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);

            if (usuario is null) return NotFound();

            if (usuario.Email == emailActual)
            {
                TempData["Error"] = "No puedes eliminar tu propia cuenta.";
                return RedirectToAction(nameof(Index));
            }

            await usuarioRepository.DeleteAsync(id);
            TempData["Exito"] = $"Usuario '{usuario.Nombre}' eliminado.";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarRolesAsync()
        {
            // Admin no puede crear otro Admin desde aquí
            var roles = await db.RolesComandas
                .Where(r => r.Nombre != "Admin")
                .ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Nombre");
        }
    }

    public class UsuarioCreateVM
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Correo inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "Mínimo 8 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona un rol.")]
        public int RolId { get; set; }
    }
}
