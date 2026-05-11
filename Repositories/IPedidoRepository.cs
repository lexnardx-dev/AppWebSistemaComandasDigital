using AppWebSistemaComandasDigital.Models;

namespace AppWebSistemaComandasDigital.Repositories
{
    public interface IPedidoRepository
    {
        Task<IEnumerable<Pedido>> GetAllAsync();
        Task<Pedido?> GetByIdAsync(int id);
        Task<IEnumerable<Pedido>> GetByMesaAsync(int mesaId);
        Task<IEnumerable<Pedido>> GetByEstadoAsync(EstadoPedido estado);
        Task<IEnumerable<Pedido>> GetByUsuarioAsync(int usuarioId);
        Task<Pedido> CreateAsync(Pedido pedido);
        Task<Pedido> UpdateAsync(Pedido pedido);
        Task<bool> UpdateEstadoAsync(int id, EstadoPedido estado);
        Task<bool> ExisteAsync(int id);
    }
}
