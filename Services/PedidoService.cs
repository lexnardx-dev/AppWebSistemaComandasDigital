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

        public async Task<(bool Exitoso, string Mensaje, PedidoDTO? Data)> CreateAsync(
            PedidoCreateDTO dto, int usuarioId)
        {
            var mesa = await mesaRepository.GetByIdAsync(dto.MesaId);
            if (mesa is null)
                return (false, "Mesa no encontrada.", null);
            if (mesa.Estado == EstadoMesa.Ocupada)
                return (false, "La mesa ya está ocupada.", null);

            var detalles = new List<DetallePedido>();
            decimal total = 0;

            foreach (var item in dto.Detalles)
            {
                var plato = await platoRepository.GetByIdAsync(item.PlatoId);
                if (plato is null)
                    return (false, $"Plato con Id {item.PlatoId} no encontrado.", null);
                if (!plato.Disponible)
                    return (false, $"El plato '{plato.Nombre}' no está disponible.", null);

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

            // Usar EstadoEnum que parsea el string internamente
            var estadoEnum = dto.EstadoEnum;
            await pedidoRepository.UpdateEstadoAsync(id, estadoEnum);

            // Liberar mesa si fue entregado o cancelado
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

        // ── Mapper ───────────────────────────────────────────────────
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
                Id             = d.Id,
                PlatoId        = d.PlatoId,
                PlatoNombre    = d.Plato?.Nombre ?? string.Empty,
                Cantidad       = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal       = d.Subtotal,
                Notas          = d.Notas
            }).ToList()
        };
    }
}
