using AppWebSistemaComandasDigital.Data;
using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Models;
using Microsoft.EntityFrameworkCore;

namespace AppWebSistemaComandasDigital.Services
{
    public class NotificacionService(ApplicationDbContext db)
    {
        public async Task CrearParaRolesAsync(
            IEnumerable<string> roles,
            string tipo,
            string titulo,
            string mensaje,
            PrioridadNotificacion prioridad = PrioridadNotificacion.Normal,
            string? enlaceRelacionado = null)
        {
            var rolesNormalizados = roles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            var usuarios = await db.Usuarios
                .Where(u => u.Activo && rolesNormalizados.Contains(u.Rol.Nombre))
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var usuarioId in usuarios)
            {
                db.Notificaciones.Add(new Notificacion
                {
                    Tipo = tipo,
                    Titulo = titulo,
                    Mensaje = mensaje,
                    Prioridad = prioridad,
                    UsuarioDestinoId = usuarioId,
                    EnlaceRelacionado = enlaceRelacionado,
                    FechaCreacion = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync();
        }

        public async Task<NotificacionesViewDTO> ObtenerAsync(
            int usuarioId,
            string? tipo,
            string? prioridad,
            string? estado)
        {
            var query = db.Notificaciones.AsNoTracking()
                .Where(n => n.UsuarioDestinoId == usuarioId || n.UsuarioDestinoId == null);

            if (!string.IsNullOrWhiteSpace(tipo))
                query = query.Where(n => n.Tipo == tipo);

            if (Enum.TryParse<PrioridadNotificacion>(prioridad, true, out var prioridadEnum))
                query = query.Where(n => n.Prioridad == prioridadEnum);

            if (estado == "leidas") query = query.Where(n => n.Leida);
            if (estado == "no-leidas") query = query.Where(n => !n.Leida);

            return new NotificacionesViewDTO
            {
                Notificaciones = await query.OrderByDescending(n => n.FechaCreacion).Take(100).ToListAsync(),
                Tipo = tipo,
                Prioridad = prioridad,
                Estado = estado,
                NoLeidas = await ContarNoLeidasAsync(usuarioId)
            };
        }

        public Task<int> ContarNoLeidasAsync(int usuarioId) =>
            db.Notificaciones.CountAsync(n =>
                !n.Leida && (n.UsuarioDestinoId == usuarioId || n.UsuarioDestinoId == null));

        public async Task MarcarTodasLeidasAsync(int usuarioId)
        {
            await db.Notificaciones
                .Where(n => !n.Leida && n.UsuarioDestinoId == usuarioId)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true));
        }

        public async Task<Notificacion?> MarcarLeidaAsync(int id, int usuarioId)
        {
            var notificacion = await db.Notificaciones.FirstOrDefaultAsync(n =>
                n.Id == id && (n.UsuarioDestinoId == usuarioId || n.UsuarioDestinoId == null));
            if (notificacion is null) return null;
            notificacion.Leida = true;
            await db.SaveChangesAsync();
            return notificacion;
        }
    }
}
