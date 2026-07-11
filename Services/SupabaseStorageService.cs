using AppWebSistemaComandasDigital.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace AppWebSistemaComandasDigital.Services
{
    public interface ISupabaseStorageService
    {
        bool EstaConfigurado { get; }
        Task<string> SubirImagenAsync(IFormFile archivo, string bucket, CancellationToken cancellationToken = default);
    }

    public class SupabaseStorageService(
        HttpClient httpClient,
        IOptions<SupabaseStorageOptions> options) : ISupabaseStorageService
    {
        private static readonly HashSet<string> TiposPermitidos =
            ["image/jpeg", "image/png", "image/webp"];

        private readonly SupabaseStorageOptions config = options.Value;

        public bool EstaConfigurado =>
            Uri.TryCreate(config.Url, UriKind.Absolute, out _) &&
            !string.IsNullOrWhiteSpace(config.ServiceRoleKey);

        public async Task<string> SubirImagenAsync(
            IFormFile archivo,
            string bucket,
            CancellationToken cancellationToken = default)
        {
            if (!EstaConfigurado)
                throw new InvalidOperationException(
                    "Supabase Storage no está configurado. Agrega SupabaseStorage__Url y SupabaseStorage__ServiceRoleKey en Railway.");

            if (archivo.Length == 0 || archivo.Length > config.MaxFileSizeBytes)
                throw new InvalidOperationException("La imagen debe pesar entre 1 byte y 5 MB.");

            if (!TiposPermitidos.Contains(archivo.ContentType.ToLowerInvariant()))
                throw new InvalidOperationException("Solo se permiten imágenes JPG, PNG o WebP.");

            var extension = archivo.ContentType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => throw new InvalidOperationException("Formato de imagen no permitido.")
            };

            var ruta = $"{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{extension}";
            var baseUrl = config.Url.TrimEnd('/');
            var endpoint = $"{baseUrl}/storage/v1/object/{Uri.EscapeDataString(bucket)}/{ruta}";

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ServiceRoleKey);
            request.Headers.Add("apikey", config.ServiceRoleKey);
            request.Headers.Add("x-upsert", "false");

            await using var stream = archivo.OpenReadStream();
            request.Content = new StreamContent(stream);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(archivo.ContentType);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var detalle = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"Supabase Storage rechazó la imagen ({(int)response.StatusCode}): {detalle}");
            }

            return $"{baseUrl}/storage/v1/object/public/{Uri.EscapeDataString(bucket)}/{ruta}";
        }
    }
}
