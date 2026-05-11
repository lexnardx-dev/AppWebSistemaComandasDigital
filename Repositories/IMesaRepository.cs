using AppWebSistemaComandasDigital.Models;

namespace AppWebSistemaComandasDigital.Repositories
{
    public interface IMesaRepository
    {
        Task<IEnumerable<Mesa>> GetAllAsync();
        Task<Mesa?> GetByIdAsync(int id);
        Task<Mesa?> GetByNumeroAsync(string numero);
        Task<IEnumerable<Mesa>> GetByEstadoAsync(EstadoMesa estado);
        Task<Mesa> CreateAsync(Mesa mesa);
        Task<Mesa> UpdateAsync(Mesa mesa);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExisteAsync(int id);
    }
}
