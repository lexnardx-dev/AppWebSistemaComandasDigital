using AppWebSistemaComandasDigital.Models;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWebSistemaComandasDigital.Areas.Cocina.Controllers
{
    [Area("Cocina")]
    [Authorize(Policy = "Cocina")]
    public class CocinaController(PedidoService pedidoService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var pendientes = await pedidoService.GetByEstadoAsync(EstadoPedido.Pendiente);
            var enCocina   = await pedidoService.GetByEstadoAsync(EstadoPedido.EnCocina);
            ViewBag.Pendientes = pendientes;
            ViewBag.EnCocina   = enCocina;
            return View();
        }
    }
}
