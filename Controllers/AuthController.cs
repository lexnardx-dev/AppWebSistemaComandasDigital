using AppWebSistemaComandasDigital.Data;
using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Models;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppWebSistemaComandasDigital.Controllers
{
    public class AuthController(AuthService authService, ApplicationDbContext db) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectSegunRol();

            ViewBag.ModoSetup = !await AdminExisteAsync();
            return View(new LoginDTO());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ModoSetup = !await AdminExisteAsync();
                return View(dto);
            }

            var (exitoso, mensaje, data) = await authService.LoginAsync(dto);

            if (!exitoso)
            {
                ModelState.AddModelError(string.Empty, mensaje);
                ViewBag.ModoSetup = !await AdminExisteAsync();
                return View(dto);
            }

            // Bloquear Mozos — solo usan la app móvil
            if (data!.Rol == "Mozo")
            {
                ModelState.AddModelError(string.Empty,
                    "Los mozos deben usar la aplicación móvil para ingresar.");
                ViewBag.ModoSetup = !await AdminExisteAsync();
                return View(dto);
            }

            Response.Cookies.Append("jwt", data.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure   = true,
                SameSite = SameSiteMode.Strict,
                Expires  = data.Expiracion
            });

            return data.Rol switch
            {
                "Admin"  => RedirectToAction("Index", "Home", new { area = "" }),
                "Cocina" => RedirectToAction("Index", "Cocina",    new { area = "Cocina" }),
                _        => RedirectToAction("Index", "Home")
            };
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(SetupInputModel model)
        {
            if (await AdminExisteAsync())
            {
                TempData["Error"] = "Ya existe un administrador registrado.";
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ModoSetup  = true;
                ViewBag.ErrorSetup = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .FirstOrDefault()?.ErrorMessage;
                return View("Login", new LoginDTO());
            }

            if (model.Password != model.ConfirmarPassword)
            {
                ViewBag.ModoSetup  = true;
                ViewBag.ErrorSetup = "Las contraseñas no coinciden.";
                return View("Login", new LoginDTO());
            }

            await SeedRolesAsync();

            var rolAdmin = await db.RolesComandas.FirstAsync(r => r.Nombre == "Admin");

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

            TempData["Exito"] = $"Cuenta creada. Bienvenido, {model.Nombre}.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        private IActionResult RedirectSegunRol()
        {
            var rol = User.FindFirst("rol_nombre")?.Value;
            return rol switch
            {
                "Admin"  => RedirectToAction("Index", "Home", new { area = "" }),
                "Cocina" => RedirectToAction("Index", "Cocina",    new { area = "Cocina" }),
                _        => RedirectToAction(nameof(Login))
            };
        }

        private async Task<bool> AdminExisteAsync() =>
            await db.Usuarios.AnyAsync(u => u.Rol.Nombre == "Admin");

        private async Task SeedRolesAsync()
        {
            var roles = new[] { "Admin", "Mozo", "Cocina" };
            foreach (var nombre in roles)
                if (!await db.RolesComandas.AnyAsync(r => r.Nombre == nombre))
                    db.RolesComandas.Add(new Rol { Nombre = nombre });
            await db.SaveChangesAsync();
        }
    }

    public class SetupInputModel
    {
        public string Nombre            { get; set; } = string.Empty;
        public string Email             { get; set; } = string.Empty;
        public string Password          { get; set; } = string.Empty;
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}
