using AppWebSistemaComandasDigital.Models;

namespace AppWebSistemaComandasDigital.Repositories
{
    public interface IPlatoRepository
    {
        Task<IEnumerable<Plato>> GetAllAsync();
        Task<Plato?> GetByIdAsync(int id);
        Task<IEnumerable<Plato>> GetByCategoriaAsync(int categoriaId);
        Task<IEnumerable<Plato>> GetDisponiblesAsync();
        Task<Plato> CreateAsync(Plato plato);
        Task<Plato> UpdateAsync(Plato plato);
        Task<bool> TieneDetallePedidoAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<bool> ForceDeleteAsync(int id);
        Task<bool> ExisteAsync(int id);
    }
}
