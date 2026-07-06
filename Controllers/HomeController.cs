using AppWebSistemaComandasDigital.Models;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AppWebSistemaComandasDigital.Controllers
{
    [Authorize]
    public class HomeController(
        MesaService   mesaService,
        PedidoService pedidoService,
        PlatoService  platoService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var mesas   = (await mesaService.GetAllAsync()).ToList();
            var pedidos = (await pedidoService.GetAllAsync()).ToList();
            var platos  = (await platoService.GetAllAsync()).ToList();

            // Métricas para las tarjetas superiores
            ViewBag.TotalMesas      = mesas.Count;
            ViewBag.MesasLibres     = mesas.Count(m => m.Estado == EstadoMesa.Libre.ToString());
            ViewBag.MesasOcupadas   = mesas.Count(m => m.Estado == EstadoMesa.Ocupada.ToString());

            ViewBag.PedidosPendientes = pedidos.Count(p => p.Estado == EstadoPedido.Pendiente.ToString());
            ViewBag.PedidosEnCocina   = pedidos.Count(p => p.Estado == EstadoPedido.EnCocina.ToString());
            ViewBag.PedidosListos     = pedidos.Count(p => p.Estado == EstadoPedido.Listo.ToString());
            ViewBag.TotalVendido      = pedidos
                .Where(p => p.Estado == EstadoPedido.Entregado.ToString())
                .Sum(p => p.Total);
            ViewBag.TotalPlatos       = platos.Count;
            ViewBag.PedidosPorHora = pedidos
                .Where(p => p.FechaCreacion.Date == DateTime.Today)
                .GroupBy(p => p.FechaCreacion.Hour)
                .OrderBy(g => g.Key)
                .Select(g => new { Hora = $"{g.Key:00}:00", Cantidad = g.Count() })
                .ToList();

            // Últimos 5 pedidos activos (no entregados ni cancelados)
            ViewBag.UltimosPedidos = pedidos
                .Where(p => p.Estado != EstadoPedido.Entregado.ToString()
                         && p.Estado != EstadoPedido.Cancelado.ToString())
                .ToList();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

