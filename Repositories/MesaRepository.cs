using AppWebSistemaComandasDigital.Data;
using AppWebSistemaComandasDigital.Models;
using Microsoft.EntityFrameworkCore;

namespace AppWebSistemaComandasDigital.Repositories
{
    public class MesaRepository(ApplicationDbContext context) : IMesaRepository
    {
        public async Task<IEnumerable<Mesa>> GetAllAsync() =>
            await context.Mesas.AsNoTracking().OrderBy(m => m.Id).ToListAsync();

        public async Task<Mesa?> GetByIdAsync(int id) =>
            await context.Mesas.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

        public async Task<Mesa?> GetByNumeroAsync(string numero) =>
            await context.Mesas.AsNoTracking().FirstOrDefaultAsync(m => m.Numero == numero);

        public async Task<IEnumerable<Mesa>> GetByEstadoAsync(EstadoMesa estado) =>
            await context.Mesas.AsNoTracking().Where(m => m.Estado == estado).OrderBy(m => m.Id).ToListAsync();

        public async Task<Mesa> CreateAsync(Mesa mesa)
        {
            context.Mesas.Add(mesa);
            await context.SaveChangesAsync();
            return mesa;
        }

        public async Task<Mesa> UpdateAsync(Mesa mesa)
        {
            context.Mesas.Update(mesa);
            await context.SaveChangesAsync();
            return mesa;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var mesa = await context.Mesas.FindAsync(id);
            if (mesa is null) return false;
            context.Mesas.Remove(mesa);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteAsync(int id) =>
            await context.Mesas.AnyAsync(m => m.Id == id);
    }
}
