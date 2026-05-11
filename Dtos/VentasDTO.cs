namespace AppWebSistemaComandasDigital.Dtos
{
    public class VentasFiltroDTO
    {
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
    }

    public class PlatoVendidoDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }

    public class VentasDashboardDTO
    {
        public VentasFiltroDTO Filtro { get; set; } = new();
        public int PedidosEntregados { get; set; }
        public decimal TotalVendido { get; set; }
        public decimal TicketPromedio { get; set; }
        public int PlatosVendidos { get; set; }
        public List<PlatoVendidoDTO> TopPlatos { get; set; } = [];
        public List<PedidoDTO> UltimasVentas { get; set; } = [];
    }
}
