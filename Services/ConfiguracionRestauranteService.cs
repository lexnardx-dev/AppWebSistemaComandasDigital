using AppWebSistemaComandasDigital.Data;
using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Models;
using Microsoft.EntityFrameworkCore;

namespace AppWebSistemaComandasDigital.Services
{
    public class ConfiguracionRestauranteService(ApplicationDbContext db)
    {
        public async Task<ConfiguracionRestauranteDTO> ObtenerAsync()
        {
            var configuracion = await db.ConfiguracionRestaurante.AsNoTracking()
                .OrderBy(c => c.Id)
                .FirstOrDefaultAsync();

            return configuracion is null ? new ConfiguracionRestauranteDTO() : Mapear(configuracion);
        }

        public async Task GuardarAsync(ConfiguracionRestauranteDTO dto)
        {
            var configuracion = await db.ConfiguracionRestaurante.OrderBy(c => c.Id).FirstOrDefaultAsync();
            if (configuracion is null)
            {
                configuracion = new ConfiguracionRestaurante();
                db.ConfiguracionRestaurante.Add(configuracion);
            }

            configuracion.NombreRestaurante = dto.NombreRestaurante.Trim();
            configuracion.Ruc = dto.Ruc?.Trim();
            configuracion.Direccion = dto.Direccion?.Trim();
            configuracion.Telefono = dto.Telefono?.Trim();
            configuracion.Moneda = dto.Moneda.Trim();
            configuracion.IgvPorcentaje = dto.IgvPorcentaje;
            configuracion.HorarioAtencion = dto.HorarioAtencion?.Trim();
            configuracion.LogoUrl = dto.LogoUrl?.Trim();
            configuracion.ColorMarca = dto.ColorMarca.Trim();
            configuracion.FechaActualizacion = DateTime.UtcNow;

            await db.SaveChangesAsync();
        }

        private static ConfiguracionRestauranteDTO Mapear(ConfiguracionRestaurante c) => new()
        {
            NombreRestaurante = c.NombreRestaurante,
            Ruc = c.Ruc,
            Direccion = c.Direccion,
            Telefono = c.Telefono,
            Moneda = c.Moneda,
            IgvPorcentaje = c.IgvPorcentaje,
            HorarioAtencion = c.HorarioAtencion,
            LogoUrl = c.LogoUrl,
            ColorMarca = c.ColorMarca
        };
    }
}
