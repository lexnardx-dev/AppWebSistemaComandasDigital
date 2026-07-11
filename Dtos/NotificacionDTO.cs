using AppWebSistemaComandasDigital.Models;

namespace AppWebSistemaComandasDigital.Dtos
{
    public class NotificacionesViewDTO
    {
        public IReadOnlyList<Notificacion> Notificaciones { get; set; } = [];
        public string? Tipo { get; set; }
        public string? Prioridad { get; set; }
        public string? Estado { get; set; }
        public int NoLeidas { get; set; }
    }
}
