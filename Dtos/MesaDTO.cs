using System.ComponentModel.DataAnnotations;
using AppWebSistemaComandasDigital.Models;

namespace AppWebSistemaComandasDigital.Dtos
{
    // ── Entrada (Cliente → API) ──────────────────────────────────────
    public class MesaCreateDTO
    {
        [Required, MaxLength(20)]
        public string Numero { get; set; } = string.Empty;

        [Range(1, 20)]
        public int Capacidad { get; set; }
    }

    public class MesaUpdateDTO
    {
        [Range(1, 20)]
        public int Capacidad { get; set; }

        public EstadoMesa Estado { get; set; }
    }

    // ── Salida (API → Cliente) ───────────────────────────────────────
    public class MesaDTO
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public int Capacidad { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
