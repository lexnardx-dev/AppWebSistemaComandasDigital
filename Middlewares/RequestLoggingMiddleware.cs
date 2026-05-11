using System.Diagnostics;

namespace AppWebSistemaComandasDigital.Middlewares
{
    public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var sw      = Stopwatch.StartNew();
            var metodo  = context.Request.Method;
            var ruta    = context.Request.Path;

            await next(context);

            sw.Stop();
            var status = context.Response.StatusCode;

            logger.LogInformation(
                "[{Metodo}] {Ruta} → {Status} ({Ms}ms)",
                metodo, ruta, status, sw.ElapsedMilliseconds);
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app) =>
            app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
