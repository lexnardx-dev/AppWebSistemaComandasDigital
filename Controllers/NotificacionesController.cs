using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppWebSistemaComandasDigital.Controllers
{
    [Authorize]
    public class NotificacionesController(NotificacionService notificacionService) : Controller
    {
        public async Task<IActionResult> Index(string? tipo, string? prioridad, string? estado)
        {
            var usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            var model = await notificacionService.ObtenerAsync(usuarioId.Value, tipo, prioridad, estado);
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            var usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            await notificacionService.MarcarTodasLeidasAsync(usuarioId.Value);
            TempData["Exito"] = "Notificaciones marcadas como leidas.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            var usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            await notificacionService.MarcarLeidaAsync(id, usuarioId.Value);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Abrir(int id)
        {
            var usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            var notificacion = await notificacionService.MarcarLeidaAsync(id, usuarioId.Value);
            if (notificacion is null) return NotFound();

            if (!string.IsNullOrWhiteSpace(notificacion.EnlaceRelacionado)
                && Url.IsLocalUrl(notificacion.EnlaceRelacionado))
                return LocalRedirect(notificacion.EnlaceRelacionado);

            return RedirectToAction(nameof(Index));
        }

        private int? ObtenerUsuarioId()
        {
            var idClaim = User.FindFirstValue("usuario_id");
            return int.TryParse(idClaim, out var usuarioId) ? usuarioId : null;
        }
    }
}
