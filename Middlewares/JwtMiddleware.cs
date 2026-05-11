using AppWebSistemaComandasDigital.Helpers;

namespace AppWebSistemaComandasDigital.Middlewares
{
    public class JwtMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, JwtHelper jwtHelper)
        {
            // Solo actúa como fallback si el pipeline de ASP.NET (JwtBearer)
            // no autenticó al usuario — evita sobrescribir context.User dos veces
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                var token = ExtraerToken(context);

                if (!string.IsNullOrEmpty(token))
                {
                    var principal = jwtHelper.ValidarToken(token);
                    if (principal is not null)
                        context.User = principal;
                }
            }

            await next(context);
        }

        private static string? ExtraerToken(HttpContext context)
        {
            // Header: Authorization: Bearer <token>
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                return authHeader["Bearer ".Length..].Trim();

            // Cookie: jwt=<token>
            if (context.Request.Cookies.TryGetValue("jwt", out var cookieToken))
                return cookieToken;

            return null;
        }
    }

    public static class JwtMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder app) =>
            app.UseMiddleware<JwtMiddleware>();
    }
}
