using AppWebSistemaComandasDigital.Data;
using AppWebSistemaComandasDigital.Models;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AppWebSistemaComandasDigital.Controllers
{
    public class SetupController(ApplicationDbContext db) : Controller
    {
        // ── GET /setup ───────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Si ya existe un admin, bloquear acceso para siempre
            if (await AdminExisteAsync())
                return RedirectToAction("Login", "Auth");

            return View();
        }

        // ── POST /setup ──────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SetupViewModel model)
        {
            // Doble verificación — por si alguien envía POST directo
            if (await AdminExisteAsync())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View(model);

            // Asegurar que existan los roles base
            await SeedRolesAsync();

            var rolAdmin = await db.RolesComandas
                .FirstOrDefaultAsync(r => r.Nombre == "Admin");

            if (rolAdmin is null)
            {
                ModelState.AddModelError(string.Empty,
                    "Error al obtener el rol Admin. Verifica la base de datos.");
                return View(model);
            }

            db.Usuarios.Add(new Usuario
            {
                Nombre        = model.Nombre.Trim(),
                Email         = model.Email.Trim().ToLower(),
                PasswordHash  = AuthService.HashPassword(model.Password),
                RolId         = rolAdmin.Id,
                Activo        = true,
                FechaCreacion = DateTime.UtcNow
            });

            await db.SaveChangesAsync();

            TempData["Exito"] = "Cuenta de administrador creada. Ya puedes iniciar sesión.";
            return RedirectToAction("Login", "Auth");
        }

        // ── Helpers privados ─────────────────────────────────────────
        private async Task<bool> AdminExisteAsync() =>
            await db.Usuarios.AnyAsync(u => u.Rol.Nombre == "Admin");

        private async Task SeedRolesAsync()
        {
            var rolesNecesarios = new[] { "Admin", "Mozo", "Cocina" };
            foreach (var nombre in rolesNecesarios)
            {
                if (!await db.RolesComandas.AnyAsync(r => r.Nombre == nombre))
                    db.RolesComandas.Add(new Rol { Nombre = nombre });
            }
            await db.SaveChangesAsync();
        }
    }

    // ── ViewModel ────────────────────────────────────────────────────
    public class SetupViewModel
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

        [Required(ErrorMessage = "Confirma tu contraseña.")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}
