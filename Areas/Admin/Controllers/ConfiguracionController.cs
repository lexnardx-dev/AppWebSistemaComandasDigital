using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWebSistemaComandasDigital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "SoloAdmin")]
    public class ConfiguracionController(ConfiguracionRestauranteService configuracionService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var configuracion = await configuracionService.ObtenerAsync();
            return View(configuracion);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ConfiguracionRestauranteDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await configuracionService.GuardarAsync(model);
            TempData["Exito"] = "Configuracion del restaurante actualizada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
