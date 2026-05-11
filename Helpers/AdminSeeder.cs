using AppWebSistemaComandasDigital.Data;
using AppWebSistemaComandasDigital.Models;
using AppWebSistemaComandasDigital.Services;
using Microsoft.EntityFrameworkCore;

namespace AppWebSistemaComandasDigital.Helpers
{
    public static class AdminSeeder
    {
        /// <summary>
        /// Siembra roles y usuario admin al iniciar la app.
        /// Solo inserta si no existen — seguro para correr múltiples veces.
        /// </summary>
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Aplicar migraciones pendientes automáticamente
            await db.Database.MigrateAsync();

            // ── Seed Roles ───────────────────────────────────────────
            var rolesNecesarios = new[] { "Admin", "Mozo", "Cocina" };

            foreach (var nombreRol in rolesNecesarios)
            {
                if (!await db.RolesComandas.AnyAsync(r => r.Nombre == nombreRol))
                {
                    db.RolesComandas.Add(new Rol { Nombre = nombreRol });
                }
            }

            await db.SaveChangesAsync();

            // ── Seed Usuario Admin ───────────────────────────────────
            const string emailAdmin = "admin@comandas.com";

            if (!await db.Usuarios.AnyAsync(u => u.Email == emailAdmin))
            {
                var rolAdmin = await db.RolesComandas
                    .FirstAsync(r => r.Nombre == "Admin");

                db.Usuarios.Add(new Usuario
                {
                    Nombre        = "Administrador",
                    Email         = emailAdmin,
                    PasswordHash  = AuthService.HashPassword("Admin2026!"),
                    RolId         = rolAdmin.Id,
                    Activo        = true,
                    FechaCreacion = DateTime.UtcNow
                });

                await db.SaveChangesAsync();
            }
        }
    }
}
