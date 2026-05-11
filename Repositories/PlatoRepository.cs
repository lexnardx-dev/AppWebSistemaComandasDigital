using AppWebSistemaComandasDigital.Data;
using AppWebSistemaComandasDigital.Models;
using Microsoft.EntityFrameworkCore;

namespace AppWebSistemaComandasDigital.Repositories
{
    public class PlatoRepository(ApplicationDbContext context) : IPlatoRepository
    {
        public async Task<IEnumerable<Plato>> GetAllAsync() =>
            await context.Platos.AsNoTracking()
                .Include(p => p.Categoria)
                .ToListAsync();

        public async Task<Plato?> GetByIdAsync(int id) =>
            await context.Platos.AsNoTracking()
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<Plato>> GetByCategoriaAsync(int categoriaId) =>
            await context.Platos.AsNoTracking()
                .Where(p => p.CategoriaId == categoriaId)
                .Include(p => p.Categoria)
                .ToListAsync();

        public async Task<IEnumerable<Plato>> GetDisponiblesAsync() =>
            await context.Platos.AsNoTracking()
                .Where(p => p.Disponible)
                .Include(p => p.Categoria)
                .ToListAsync();

        public async Task<Plato> CreateAsync(Plato plato)
        {
            context.Platos.Add(plato);
            await context.SaveChangesAsync();
            return plato;
        }

        public async Task<Plato> UpdateAsync(Plato plato)
        {
            var existente = await context.Platos.FirstOrDefaultAsync(p => p.Id == plato.Id);
            if (existente is null)
                throw new InvalidOperationException("Plato no encontrado.");

            existente.Nombre      = plato.Nombre;
            existente.Descripcion = plato.Descripcion;
            existente.Precio      = plato.Precio;
            existente.Disponible  = plato.Disponible;
            existente.ImagenUrl   = plato.ImagenUrl;
            existente.CategoriaId = plato.CategoriaId;

            await context.SaveChangesAsync();
            return existente;
        }

        public async Task<bool> TieneDetallePedidoAsync(int id) =>
            await context.DetallesPedido.AnyAsync(d => d.PlatoId == id);

        public async Task<bool> DeleteAsync(int id)
        {
            var plato = await context.Platos.FindAsync(id);
            if (plato is null) return false;
            context.Platos.Remove(plato);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Elimina el plato junto con todos sus DetallesPedido
        /// asociados — en una sola transacción.
        /// </summary>
        public async Task<bool> ForceDeleteAsync(int id)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // 1. Eliminar DetallesPedido que referencian este plato
                var detalles = await context.DetallesPedido
                    .Where(d => d.PlatoId == id)
                    .ToListAsync();

                if (detalles.Count > 0)
                {
                    context.DetallesPedido.RemoveRange(detalles);
                    await context.SaveChangesAsync();
                }

                // 2. Eliminar el plato
                var plato = await context.Platos.FindAsync(id);
                if (plato is null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                context.Platos.Remove(plato);
                await context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ExisteAsync(int id) =>
            await context.Platos.AnyAsync(p => p.Id == id);
    }
}
