using System.Globalization;
using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Models;
using AppWebSistemaComandasDigital.Repositories;

namespace AppWebSistemaComandasDigital.Services
{
    public class PedidoService(
        IPedidoRepository pedidoRepository,
        IMesaRepository   mesaRepository,
        IPlatoRepository  platoRepository)
    {
        private static readonly CultureInfo CulturaPeru = new("es-PE");

        public async Task<IEnumerable<PedidoDTO>> GetAllAsync()
        {
            var pedidos = await pedidoRepository.GetAllAsync();
            return pedidos.Select(MapToDTO);
        }

        public async Task<PedidoDTO?> GetByIdAsync(int id)
        {
            var pedido = await pedidoRepository.GetByIdAsync(id);
            return pedido is null ? null : MapToDTO(pedido);
        }

        public async Task<IEnumerable<PedidoDTO>> GetByEstadoAsync(EstadoPedido estado)
        {
            var pedidos = await pedidoRepository.GetByEstadoAsync(estado);
            return pedidos.Select(MapToDTO);
        }

        public async Task<IEnumerable<PedidoDTO>> GetByMesaAsync(int mesaId)
        {
            var pedidos = await pedidoRepository.GetByMesaAsync(mesaId);
            return pedidos.Select(MapToDTO);
        }

        public async Task<VentasDashboardDTO> GetVentasDashboardAsync(DateTime? desde, DateTime? hasta)
        {
            var entregados = (await GetAllAsync())
                .Where(p => p.Estado == nameof(EstadoPedido.Entregado))
                .ToList();

            var ventas = AplicarRango(entregados, desde, hasta)
                .OrderByDescending(p => p.FechaCreacion)
                .ToList();

            var detalles = ventas.SelectMany(p => p.Detalles).ToList();
            var total = ventas.Sum(p => p.Total);

            var topPlatos = detalles
                .GroupBy(d => string.IsNullOrWhiteSpace(d.PlatoNombre) ? "Sin nombre" : d.PlatoNombre)
                .Select(g => new PlatoVendidoDTO
                {
                    Nombre = g.Key,
                    Cantidad = g.Sum(d => d.Cantidad),
                    Total = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(p => p.Total)
                .ThenByDescending(p => p.Cantidad)
                .Take(12)
                .ToList();

            var ventasPorCategoria = detalles
                .GroupBy(d => string.IsNullOrWhiteSpace(d.CategoriaNombre) ? "Sin categoria" : d.CategoriaNombre)
                .Select(g => new CategoriaVentaDTO
                {
                    Nombre = g.Key,
                    Cantidad = g.Sum(d => d.Cantidad),
                    Total = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(c => c.Total)
                .ThenByDescending(c => c.Cantidad)
                .Take(7)
                .ToList();

            var evolucion = ventas
                .GroupBy(p => p.FechaCreacion.Date)
                .OrderBy(g => g.Key)
                .Select(g => new VentasSerieDTO
                {
                    Fecha = g.Key,
                    Etiqueta = g.Key.ToString("dd/MM", CulturaPeru),
                    Total = g.Sum(p => p.Total),
                    Pedidos = g.Count()
                })
                .ToList();

            var ventasPorMozo = ventas
                .GroupBy(p => string.IsNullOrWhiteSpace(p.UsuarioNombre) ? "Sin mozo" : p.UsuarioNombre)
                .Select(g => new MozoVentaDTO
                {
                    Nombre = g.Key,
                    Pedidos = g.Count(),
                    Total = g.Sum(p => p.Total)
                })
                .OrderByDescending(m => m.Total)
                .ThenByDescending(m => m.Pedidos)
                .Take(6)
                .ToList();

            var ventasPorHora = ventas
                .GroupBy(p => p.FechaCreacion.Hour)
                .OrderBy(g => g.Key)
                .Select(g => new HoraVentaDTO
                {
                    Hora = g.Key,
                    Etiqueta = $"{g.Key:00}:00",
                    Pedidos = g.Count(),
                    Total = g.Sum(p => p.Total)
                })
                .ToList();

            var ventasPorDiaSemana = ventas
                .GroupBy(p => p.FechaCreacion.DayOfWeek)
                .Select(g => new DiaSemanaVentaDTO
                {
                    Orden = OrdenDia(g.Key),
                    Dia = NombreDia(g.Key),
                    Pedidos = g.Count(),
                    Total = g.Sum(p => p.Total)
                })
                .OrderBy(d => d.Orden)
                .ToList();

            return new VentasDashboardDTO
            {
                Filtro = new VentasFiltroDTO { Desde = desde, Hasta = hasta },
                PedidosEntregados = ventas.Count,
                TotalVendido = total,
                TicketPromedio = ventas.Count == 0 ? 0 : total / ventas.Count,
                PlatosVendidos = detalles.Sum(d => d.Cantidad),
                TopPlatos = topPlatos,
                EvolucionVentas = evolucion,
                VentasPorCategoria = ventasPorCategoria,
                VentasPorMozo = ventasPorMozo,
                VentasPorHora = ventasPorHora,
                VentasPorDiaSemana = ventasPorDiaSemana,
                Comparativo = CalcularComparativo(entregados, ventas, desde, hasta)
            };
        }

        public async Task<(bool Exitoso, string Mensaje, PedidoDTO? Data)> CreateAsync(
            PedidoCreateDTO dto, int usuarioId)
        {
            var mesa = await mesaRepository.GetByIdAsync(dto.MesaId);
            if (mesa is null)
                return (false, "Mesa no encontrada.", null);
            if (mesa.Estado == EstadoMesa.Ocupada)
                return (false, "La mesa ya esta ocupada.", null);

            var detalles = new List<DetallePedido>();
            decimal total = 0;

            foreach (var item in dto.Detalles)
            {
                var plato = await platoRepository.GetByIdAsync(item.PlatoId);
                if (plato is null)
                    return (false, $"Plato con Id {item.PlatoId} no encontrado.", null);
                if (!plato.Disponible)
                    return (false, $"El plato '{plato.Nombre}' no esta disponible.", null);

                var subtotal = plato.Precio * item.Cantidad;
                total += subtotal;

                detalles.Add(new DetallePedido
                {
                    PlatoId        = plato.Id,
                    Cantidad       = item.Cantidad,
                    PrecioUnitario = plato.Precio,
                    Subtotal       = subtotal,
                    Notas          = item.Notas?.Trim()
                });
            }

            var pedido = new Pedido
            {
                MesaId        = dto.MesaId,
                UsuarioId     = usuarioId,
                Observaciones = dto.Observaciones?.Trim(),
                Estado        = EstadoPedido.Pendiente,
                Total         = total,
                FechaCreacion = DateTime.UtcNow,
                Detalles      = detalles
            };

            var creado = await pedidoRepository.CreateAsync(pedido);

            mesa.Estado = EstadoMesa.Ocupada;
            await mesaRepository.UpdateAsync(mesa);

            var resultado = await pedidoRepository.GetByIdAsync(creado.Id);
            return (true, "Pedido creado correctamente.", MapToDTO(resultado!));
        }

        public async Task<(bool Exitoso, string Mensaje)> UpdateEstadoAsync(
            int id, PedidoUpdateEstadoDTO dto)
        {
            var pedido = await pedidoRepository.GetByIdAsync(id);
            if (pedido is null)
                return (false, "Pedido no encontrado.");

            var estadoEnum = dto.EstadoEnum;
            await pedidoRepository.UpdateEstadoAsync(id, estadoEnum);

            if (estadoEnum is EstadoPedido.Entregado or EstadoPedido.Cancelado)
            {
                var mesa = await mesaRepository.GetByIdAsync(pedido.MesaId);
                if (mesa is not null)
                {
                    mesa.Estado = EstadoMesa.Libre;
                    await mesaRepository.UpdateAsync(mesa);
                }
            }

            return (true, $"Estado actualizado a '{dto.Estado}'.");
        }

        private static IEnumerable<PedidoDTO> AplicarRango(IEnumerable<PedidoDTO> pedidos, DateTime? desde, DateTime? hasta)
        {
            var filtrados = pedidos;

            if (desde.HasValue)
                filtrados = filtrados.Where(p => p.FechaCreacion.Date >= desde.Value.Date);

            if (hasta.HasValue)
                filtrados = filtrados.Where(p => p.FechaCreacion.Date <= hasta.Value.Date);

            return filtrados;
        }

        private static ComparativoVentasDTO CalcularComparativo(
            List<PedidoDTO> todosEntregados,
            List<PedidoDTO> ventasActuales,
            DateTime? desde,
            DateTime? hasta)
        {
            var inicioActual = (desde ?? ventasActuales.OrderBy(p => p.FechaCreacion).FirstOrDefault()?.FechaCreacion ?? DateTime.Today).Date;
            var finActual = (hasta ?? ventasActuales.OrderByDescending(p => p.FechaCreacion).FirstOrDefault()?.FechaCreacion ?? DateTime.Today).Date;
            var diasPeriodo = Math.Max(1, (finActual - inicioActual).Days + 1);

            var finAnterior = inicioActual.AddDays(-1);
            var inicioAnterior = finAnterior.AddDays(-(diasPeriodo - 1));

            var ventasAnteriores = todosEntregados
                .Where(p => p.FechaCreacion.Date >= inicioAnterior && p.FechaCreacion.Date <= finAnterior)
                .ToList();

            var totalActual = ventasActuales.Sum(p => p.Total);
            var totalAnterior = ventasAnteriores.Sum(p => p.Total);
            var variacion = totalAnterior == 0
                ? totalActual > 0 ? 100 : 0
                : Math.Round(((totalActual - totalAnterior) / totalAnterior) * 100, 1);

            return new ComparativoVentasDTO
            {
                PeriodoActual = $"{inicioActual:dd/MM} - {finActual:dd/MM}",
                PeriodoAnterior = $"{inicioAnterior:dd/MM} - {finAnterior:dd/MM}",
                TotalActual = totalActual,
                TotalAnterior = totalAnterior,
                PedidosActuales = ventasActuales.Count,
                PedidosAnteriores = ventasAnteriores.Count,
                VariacionPorcentual = variacion,
                Tendencia = variacion > 0 ? "Subio" : variacion < 0 ? "Bajo" : "Sin variacion"
            };
        }

        private static int OrdenDia(DayOfWeek dia) => dia switch
        {
            DayOfWeek.Monday => 1,
            DayOfWeek.Tuesday => 2,
            DayOfWeek.Wednesday => 3,
            DayOfWeek.Thursday => 4,
            DayOfWeek.Friday => 5,
            DayOfWeek.Saturday => 6,
            _ => 7
        };

        private static string NombreDia(DayOfWeek dia) => dia switch
        {
            DayOfWeek.Monday => "Lunes",
            DayOfWeek.Tuesday => "Martes",
            DayOfWeek.Wednesday => "Miercoles",
            DayOfWeek.Thursday => "Jueves",
            DayOfWeek.Friday => "Viernes",
            DayOfWeek.Saturday => "Sabado",
            _ => "Domingo"
        };

        private static PedidoDTO MapToDTO(Pedido p) => new()
        {
            Id            = p.Id,
            MesaId        = p.MesaId,
            MesaNumero    = p.Mesa?.Numero    ?? string.Empty,
            UsuarioId     = p.UsuarioId,
            UsuarioNombre = p.Usuario?.Nombre ?? string.Empty,
            Estado        = p.Estado.ToString(),
            Observaciones = p.Observaciones,
            Total         = p.Total,
            FechaCreacion = p.FechaCreacion,
            Detalles      = p.Detalles.Select(d => new DetallePedidoDTO
            {
                Id              = d.Id,
                PlatoId         = d.PlatoId,
                PlatoNombre     = d.Plato?.Nombre ?? string.Empty,
                CategoriaId     = d.Plato?.CategoriaId ?? 0,
                CategoriaNombre = d.Plato?.Categoria?.Nombre ?? string.Empty,
                Cantidad        = d.Cantidad,
                PrecioUnitario  = d.PrecioUnitario,
                Subtotal        = d.Subtotal,
                Notas           = d.Notas
            }).ToList()
        };
    }
}