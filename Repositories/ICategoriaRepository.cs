using AppWebSistemaComandasDigital.Models;

namespace AppWebSistemaComandasDigital.Repositories
{
    public interface ICategoriaRepository
    {
        Task<IEnumerable<Categoria>> GetAllAsync();
        Task<Categoria?> GetByIdAsync(int id);
        Task<Categoria?> GetByNombreAsync(string nombre);
        Task<IEnumerable<Categoria>> GetActivasAsync();
        Task<bool> TienePlatosAsync(int id);
        Task<Categoria> CreateAsync(Categoria categoria);
        Task<Categoria> UpdateAsync(Categoria categoria);
        Task<bool> DeleteAsync(int id);
        Task<bool> ForceDeleteAsync(int id);
        Task<bool> ExisteAsync(int id);
    }
}
