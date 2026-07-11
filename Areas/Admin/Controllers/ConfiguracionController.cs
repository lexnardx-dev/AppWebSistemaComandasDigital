using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Services;
using AppWebSistemaComandasDigital.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AppWebSistemaComandasDigital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "SoloAdmin")]
    public class ConfiguracionController(
        ConfiguracionRestauranteService configuracionService,
        ISupabaseStorageService storage,
        IOptions<SupabaseStorageOptions> storageOptions) : Controller
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

            if (model.LogoArchivo is not null)
            {
                try
                {
                    model.LogoUrl = await storage.SubirImagenAsync(
                        model.LogoArchivo, storageOptions.Value.Buckets.Logos, HttpContext.RequestAborted);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(nameof(model.LogoArchivo), ex.Message);
                    return View(model);
                }
            }

            await configuracionService.GuardarAsync(model);
            TempData["Exito"] = "Configuración del restaurante actualizada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
