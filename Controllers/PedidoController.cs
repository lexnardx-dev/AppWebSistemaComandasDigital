using AppWebSistemaComandasDigital.Models;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWebSistemaComandasDigital.Controllers
{
    [Authorize]
    public class PedidoController(
        PedidoService pedidoService,
        MesaService mesaService,
        PlatoService platoService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var pedidos = await pedidoService.GetAllAsync();
            var activos = pedidos
                .Where(p => p.Estado is nameof(EstadoPedido.Pendiente)
                    or nameof(EstadoPedido.EnCocina)
                    or nameof(EstadoPedido.Listo))
                .OrderByDescending(p => p.Id);

            return View(activos);
        }

        public async Task<IActionResult> Historial(DateTime? desde, DateTime? hasta, string? estado, string? mesa, string? mozo)
        {
            var pedidos = (await pedidoService.GetAllAsync())
                .Where(p => p.Estado is nameof(EstadoPedido.Entregado) or nameof(EstadoPedido.Cancelado));

            if (desde.HasValue)
                pedidos = pedidos.Where(p => p.FechaCreacion.Date >= desde.Value.Date);

            if (hasta.HasValue)
                pedidos = pedidos.Where(p => p.FechaCreacion.Date <= hasta.Value.Date);

            if (!string.IsNullOrWhiteSpace(estado))
                pedidos = pedidos.Where(p => p.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(mesa))
                pedidos = pedidos.Where(p => p.MesaNumero.Contains(mesa.Trim(), StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(mozo))
                pedidos = pedidos.Where(p => p.UsuarioNombre.Contains(mozo.Trim(), StringComparison.OrdinalIgnoreCase));

            ViewBag.Desde = desde?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.Estado = estado ?? "";
            ViewBag.Mesa = mesa ?? "";
            ViewBag.Mozo = mozo ?? "";

            return View(pedidos.OrderByDescending(p => p.FechaCreacion).ToList());
        }

        public async Task<IActionResult> Crear(int? mesaId = null)
        {
            ViewBag.Mesas = await mesaService.GetLibresAsync();
            ViewBag.Platos = await platoService.GetDisponiblesAsync();
            ViewBag.MesaIdFijo = mesaId;
            return View();
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var pedido = await pedidoService.GetByIdAsync(id);
            if (pedido is null) return NotFound();
            return View(pedido);
        }
    }
}
