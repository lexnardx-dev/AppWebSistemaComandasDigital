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

    public class VentasSerieDTO
    {
        public string Etiqueta { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public int Pedidos { get; set; }
    }

    public class CategoriaVentaDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }

    public class MozoVentaDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public int Pedidos { get; set; }
        public decimal Total { get; set; }
    }

    public class HoraVentaDTO
    {
        public int Hora { get; set; }
        public string Etiqueta { get; set; } = string.Empty;
        public int Pedidos { get; set; }
        public decimal Total { get; set; }
    }

    public class DiaSemanaVentaDTO
    {
        public int Orden { get; set; }
        public string Dia { get; set; } = string.Empty;
        public int Pedidos { get; set; }
        public decimal Total { get; set; }
    }

    public class ComparativoVentasDTO
    {
        public string PeriodoActual { get; set; } = string.Empty;
        public string PeriodoAnterior { get; set; } = string.Empty;
        public decimal TotalActual { get; set; }
        public decimal TotalAnterior { get; set; }
        public int PedidosActuales { get; set; }
        public int PedidosAnteriores { get; set; }
        public decimal VariacionPorcentual { get; set; }
        public string Tendencia { get; set; } = "Sin variacion";
    }

    public class VentasDashboardDTO
    {
        public VentasFiltroDTO Filtro { get; set; } = new();
        public int PedidosEntregados { get; set; }
        public decimal TotalVendido { get; set; }
        public decimal TicketPromedio { get; set; }
        public int PlatosVendidos { get; set; }
        public List<PlatoVendidoDTO> TopPlatos { get; set; } = [];
        public List<VentasSerieDTO> EvolucionVentas { get; set; } = [];
        public List<CategoriaVentaDTO> VentasPorCategoria { get; set; } = [];
        public List<MozoVentaDTO> VentasPorMozo { get; set; } = [];
        public List<HoraVentaDTO> VentasPorHora { get; set; } = [];
        public List<DiaSemanaVentaDTO> VentasPorDiaSemana { get; set; } = [];
        public ComparativoVentasDTO Comparativo { get; set; } = new();

        // Se conserva por compatibilidad con cualquier vista antigua que lo consuma.
        public List<PedidoDTO> UltimasVentas { get; set; } = [];
    }
}