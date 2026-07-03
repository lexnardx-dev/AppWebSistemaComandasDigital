using AppWebSistemaComandasDigital.Data;
using AppWebSistemaComandasDigital.Models;
using Microsoft.EntityFrameworkCore;

namespace AppWebSistemaComandasDigital.Repositories
{
    public class PedidoRepository(ApplicationDbContext context) : IPedidoRepository
    {
        public async Task<IEnumerable<Pedido>> GetAllAsync() =>
            await context.Pedidos.AsNoTracking()
                .Include(p => p.Mesa)
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                    .ThenInclude(pl => pl.Categoria)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

        public async Task<Pedido?> GetByIdAsync(int id) =>
            await context.Pedidos.AsNoTracking()
                .Include(p => p.Mesa)
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                    .ThenInclude(pl => pl.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<Pedido>> GetByMesaAsync(int mesaId) =>
            await context.Pedidos.AsNoTracking()
                .Where(p => p.MesaId == mesaId)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                    .ThenInclude(pl => pl.Categoria)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

        public async Task<IEnumerable<Pedido>> GetByEstadoAsync(EstadoPedido estado) =>
            await context.Pedidos.AsNoTracking()
                .Where(p => p.Estado == estado)
                .Include(p => p.Mesa)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                    .ThenInclude(pl => pl.Categoria)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

        public async Task<IEnumerable<Pedido>> GetByUsuarioAsync(int usuarioId) =>
            await context.Pedidos.AsNoTracking()
                .Where(p => p.UsuarioId == usuarioId)
                .Include(p => p.Mesa)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Plato)
                    .ThenInclude(pl => pl.Categoria)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

        public async Task<Pedido> CreateAsync(Pedido pedido)
        {
            context.Pedidos.Add(pedido);
            await context.SaveChangesAsync();
            return pedido;
        }

        public async Task<Pedido> UpdateAsync(Pedido pedido)
        {
            context.Pedidos.Update(pedido);
            await context.SaveChangesAsync();
            return pedido;
        }

        public async Task<bool> UpdateEstadoAsync(int id, EstadoPedido estado)
        {
            var pedido = await context.Pedidos.FindAsync(id);
            if (pedido is null) return false;
            pedido.Estado = estado;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteAsync(int id) =>
            await context.Pedidos.AnyAsync(p => p.Id == id);
    }
}