using AppWebSistemaComandasDigital.Models;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWebSistemaComandasDigital.Controllers
{
    [Authorize]
    public class MesaController(MesaService mesaService, PedidoService pedidoService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var mesas = await mesaService.GetAllAsync();
            return View(mesas);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var mesa = await mesaService.GetByIdAsync(id);
            if (mesa is null) return NotFound();

            // Pedidos activos de esta mesa (excluye Entregados y Cancelados)
            var todosPedidos = await pedidoService.GetByMesaAsync(id);
            var pedidosActivos = todosPedidos
                .Where(p => p.Estado != EstadoPedido.Entregado.ToString()
                         && p.Estado != EstadoPedido.Cancelado.ToString())
                .ToList();

            ViewBag.PedidosActivos = pedidosActivos;
            return View(mesa);
        }
    }
}
