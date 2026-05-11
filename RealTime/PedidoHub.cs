using AppWebSistemaComandasDigital.Dtos;
using Microsoft.AspNetCore.SignalR;

namespace AppWebSistemaComandasDigital.RealTime
{
    public class PedidoHub : Hub
    {
        // Cliente se une al grupo de su rol (cocina, admin, mozo)
        public async Task UnirseAGrupo(string grupo)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, grupo);
        }

        public async Task SalirDeGrupo(string grupo)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, grupo);
        }
    }

    // Helper estático para notificar desde los Services/Controllers
    public static class PedidoHubNotificaciones
    {
        public static async Task NuevoPedido(
            IHubContext<PedidoHub> hub, PedidoDTO pedido)
        {
            // Notifica al grupo "Cocina" con el nuevo pedido
            await hub.Clients.Group("Cocina")
                .SendAsync("NuevoPedido", pedido);

            // Notifica al grupo "Admin" también
            await hub.Clients.Group("Admin")
                .SendAsync("NuevoPedido", pedido);
        }

        public static async Task EstadoActualizado(
            IHubContext<PedidoHub> hub, int pedidoId, string nuevoEstado)
        {
            await hub.Clients.All
                .SendAsync("EstadoPedidoActualizado", new { pedidoId, nuevoEstado });
        }

        public static async Task MesaActualizada(
            IHubContext<PedidoHub> hub, int mesaId, string nuevoEstado)
        {
            await hub.Clients.All
                .SendAsync("EstadoMesaActualizado", new { mesaId, nuevoEstado });
        }
    }
}
