using System.Net;
using System.Text.Json;
using AppWebSistemaComandasDigital.Helpers;

namespace AppWebSistemaComandasDigital.Middlewares
{
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error no controlado: {Message}", ex.Message);

                // Solo responde JSON para rutas /api/
                // Las rutas MVC dejan que ASP.NET maneje el error normalmente
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    await HandleApiExceptionAsync(context, ex);
                }
                else
                {
                    throw; // Re-lanza para que el ExceptionHandler de MVC lo procese
                }
            }
        }

        private static async Task HandleApiExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;

            var response = ResponseHelper.Error<object>(
                "Ocurrió un error interno en el servidor.",
                [ex.Message]);

            var json = JsonSerializer.Serialize(response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await context.Response.WriteAsync(json);
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app) =>
            app.UseMiddleware<ExceptionMiddleware>();
    }
}
