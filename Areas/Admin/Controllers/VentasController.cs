using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Models;
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
            var entregados = (await pedidoService.GetAllAsync())
                .Where(p => p.Estado == nameof(EstadoPedido.Entregado));

            if (desde.HasValue)
                entregados = entregados.Where(p => p.FechaCreacion.Date >= desde.Value.Date);

            if (hasta.HasValue)
                entregados = entregados.Where(p => p.FechaCreacion.Date <= hasta.Value.Date);

            var ventas = entregados.OrderByDescending(p => p.FechaCreacion).ToList();
            var total = ventas.Sum(p => p.Total);
            var platos = ventas
                .SelectMany(p => p.Detalles)
                .GroupBy(d => d.PlatoNombre)
                .Select(g => new PlatoVendidoDTO
                {
                    Nombre = g.Key,
                    Cantidad = g.Sum(d => d.Cantidad),
                    Total = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(p => p.Cantidad)
                .ThenByDescending(p => p.Total)
                .Take(8)
                .ToList();

            var model = new VentasDashboardDTO
            {
                Filtro = new VentasFiltroDTO { Desde = desde, Hasta = hasta },
                PedidosEntregados = ventas.Count,
                TotalVendido = total,
                TicketPromedio = ventas.Count == 0 ? 0 : total / ventas.Count,
                PlatosVendidos = ventas.SelectMany(p => p.Detalles).Sum(d => d.Cantidad),
                TopPlatos = platos,
                UltimasVentas = ventas.Take(8).ToList()
            };

            return View(model);
        }
    }
}
