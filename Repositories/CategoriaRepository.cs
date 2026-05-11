using AppWebSistemaComandasDigital.Data;
using AppWebSistemaComandasDigital.Models;
using Microsoft.EntityFrameworkCore;

namespace AppWebSistemaComandasDigital.Repositories
{
    public class CategoriaRepository(ApplicationDbContext context) : ICategoriaRepository
    {
        public async Task<IEnumerable<Categoria>> GetAllAsync() =>
            await context.Categorias.AsNoTracking()
                .OrderBy(c => c.Nombre)
                .ToListAsync();

        public async Task<Categoria?> GetByIdAsync(int id) =>
            await context.Categorias.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Categoria?> GetByNombreAsync(string nombre) =>
            await context.Categorias.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Nombre.ToLower() == nombre.ToLower());

        public async Task<IEnumerable<Categoria>> GetActivasAsync() =>
            await context.Categorias.AsNoTracking()
                .Where(c => c.Activa)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

        public async Task<bool> TienePlatosAsync(int id) =>
            await context.Platos.AnyAsync(p => p.CategoriaId == id);

        public async Task<Categoria> CreateAsync(Categoria categoria)
        {
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();
            return categoria;
        }

        public async Task<Categoria> UpdateAsync(Categoria categoria)
        {
            context.Categorias.Update(categoria);
            await context.SaveChangesAsync();
            return categoria;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var categoria = await context.Categorias.FindAsync(id);
            if (categoria is null) return false;
            context.Categorias.Remove(categoria);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Elimina la categoría junto con todos sus platos y los detalles
        /// de pedido asociados — en una sola transacción.
        /// </summary>
        public async Task<bool> ForceDeleteAsync(int id)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // 1. Obtener ids de platos de esta categoría
                var platoIds = await context.Platos
                    .Where(p => p.CategoriaId == id)
                    .Select(p => p.Id)
                    .ToListAsync();

                if (platoIds.Count > 0)
                {
                    // 2. Eliminar DetallesPedido que referencian esos platos
                    var detalles = await context.DetallesPedido
                        .Where(d => platoIds.Contains(d.PlatoId))
                        .ToListAsync();
                    context.DetallesPedido.RemoveRange(detalles);

                    // 3. Eliminar los platos
                    var platos = await context.Platos
                        .Where(p => platoIds.Contains(p.Id))
                        .ToListAsync();
                    context.Platos.RemoveRange(platos);

                    await context.SaveChangesAsync();
                }

                // 4. Eliminar la categoría
                var categoria = await context.Categorias.FindAsync(id);
                if (categoria is null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                context.Categorias.Remove(categoria);
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
            await context.Categorias.AnyAsync(c => c.Id == id);
    }
}
