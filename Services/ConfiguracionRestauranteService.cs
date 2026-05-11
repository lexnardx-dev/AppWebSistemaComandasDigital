using AppWebSistemaComandasDigital.Dtos;
using System.Text.Json;

namespace AppWebSistemaComandasDigital.Services
{
    public class ConfiguracionRestauranteService(IWebHostEnvironment environment)
    {
        private readonly JsonSerializerOptions jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private string FilePath =>
            Path.Combine(environment.ContentRootPath, "Data", "configuracion-restaurante.json");

        public async Task<ConfiguracionRestauranteDTO> ObtenerAsync()
        {
            if (!File.Exists(FilePath))
                return new ConfiguracionRestauranteDTO();

            await using var stream = File.OpenRead(FilePath);
            return await JsonSerializer.DeserializeAsync<ConfiguracionRestauranteDTO>(stream, jsonOptions)
                ?? new ConfiguracionRestauranteDTO();
        }

        public async Task GuardarAsync(ConfiguracionRestauranteDTO configuracion)
        {
            var directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            configuracion.NombreRestaurante = configuracion.NombreRestaurante.Trim();
            configuracion.Ruc = configuracion.Ruc?.Trim();
            configuracion.Direccion = configuracion.Direccion?.Trim();
            configuracion.Telefono = configuracion.Telefono?.Trim();
            configuracion.Moneda = configuracion.Moneda.Trim();
            configuracion.HorarioAtencion = configuracion.HorarioAtencion?.Trim();
            configuracion.LogoUrl = configuracion.LogoUrl?.Trim();
            configuracion.ColorMarca = configuracion.ColorMarca.Trim();

            await using var stream = File.Create(FilePath);
            await JsonSerializer.SerializeAsync(stream, configuracion, jsonOptions);
        }
    }
}
