using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWebSistemaComandasDigital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "SoloAdmin")]
    public class VentasController(PedidoService pedidoService) : Controller
    {
        public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta)
        {
            var model = await pedidoService.GetVentasDashboardAsync(desde, hasta);
            return View(model);
        }
    }
}