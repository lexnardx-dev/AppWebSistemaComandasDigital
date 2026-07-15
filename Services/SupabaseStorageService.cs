using AppWebSistemaComandasDigital.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;

namespace AppWebSistemaComandasDigital.Services
{
    public enum SupabaseStorageErrorType
    {
        NoConfigurado,
        BucketNoConfigurado,
        BucketNoExiste,
        BucketNoPublico,
        ArchivoInvalido,
        Rechazado
    }

    public sealed class SupabaseStorageException(
        SupabaseStorageErrorType type,
        string message) : InvalidOperationException(message)
    {
        public SupabaseStorageErrorType Type { get; } = type;
    }

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
                throw new SupabaseStorageException(
                    SupabaseStorageErrorType.NoConfigurado,
                    "Supabase Storage no está configurado. En Railway agrega SupabaseStorage__Url y SupabaseStorage__ServiceRoleKey.");

            if (string.IsNullOrWhiteSpace(bucket))
                throw new SupabaseStorageException(
                    SupabaseStorageErrorType.BucketNoConfigurado,
                    "No hay bucket configurado para esta imagen. Revisa SupabaseStorage__Buckets en Railway.");

            if (archivo.Length == 0 || archivo.Length > config.MaxFileSizeBytes)
                throw new SupabaseStorageException(
                    SupabaseStorageErrorType.ArchivoInvalido,
                    $"La imagen debe pesar entre 1 byte y {config.MaxFileSizeBytes / 1024 / 1024} MB.");

            if (!TiposPermitidos.Contains(archivo.ContentType.ToLowerInvariant()))
                throw new SupabaseStorageException(
                    SupabaseStorageErrorType.ArchivoInvalido,
                    "Solo se permiten imágenes JPG, PNG o WebP.");

            var extension = archivo.ContentType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => throw new SupabaseStorageException(
                    SupabaseStorageErrorType.ArchivoInvalido,
                    "Formato de imagen no permitido.")
            };

            var ruta = $"{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{extension}";
            var baseUrl = config.Url.TrimEnd('/');
            var bucketSeguro = bucket.Trim();
            var endpoint = $"{baseUrl}/storage/v1/object/{Uri.EscapeDataString(bucketSeguro)}/{ruta}";

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
                throw CrearErrorSubida(response.StatusCode, bucketSeguro, detalle);
            }

            var urlPublica = $"{baseUrl}/storage/v1/object/public/{Uri.EscapeDataString(bucketSeguro)}/{ruta}";
            await ValidarLecturaPublicaAsync(urlPublica, bucketSeguro, cancellationToken);
            return urlPublica;
        }

        private async Task ValidarLecturaPublicaAsync(
            string urlPublica,
            string bucket,
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, urlPublica);
            request.Headers.Range = new RangeHeaderValue(0, 0);

            using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (response.IsSuccessStatusCode)
                return;

            throw new SupabaseStorageException(
                SupabaseStorageErrorType.BucketNoPublico,
                $"La imagen se subió, pero el bucket '{bucket}' no permite lectura pública. Marca el bucket como público en Supabase Storage o configura URLs firmadas antes de usarlo en la interfaz.");
        }

        private static SupabaseStorageException CrearErrorSubida(
            HttpStatusCode statusCode,
            string bucket,
            string detalle)
        {
            var status = (int)statusCode;
            var detalleNormalizado = detalle.ToLowerInvariant();

            if (status == 404 || detalleNormalizado.Contains("bucket not found"))
            {
                return new SupabaseStorageException(
                    SupabaseStorageErrorType.BucketNoExiste,
                    $"El bucket '{bucket}' no existe en Supabase Storage. Créalo o corrige SupabaseStorage__Buckets.");
            }

            if (status is 401 or 403)
            {
                return new SupabaseStorageException(
                    SupabaseStorageErrorType.Rechazado,
                    "Supabase rechazó la subida. Revisa que SupabaseStorage__ServiceRoleKey sea correcto y tenga permisos de Storage.");
            }

            return new SupabaseStorageException(
                SupabaseStorageErrorType.Rechazado,
                $"Supabase Storage rechazó la imagen ({status}). Detalle: {detalle}");
        }
    }
}
