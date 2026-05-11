using AppWebSistemaComandasDigital.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AppWebSistemaComandasDigital.Helpers
{
    public class JwtHelper(IConfiguration configuration)
    {
        public string GenerarToken(Usuario usuario)
        {
            var jwtConfig = configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtConfig["Key"]!));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(ClaimTypes.Name,               usuario.Nombre),
                new Claim(ClaimTypes.Role,               usuario.Rol.Nombre),
                new Claim("rol_nombre",                  usuario.Rol.Nombre),
                new Claim("usuario_id",                  usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
            };

            var expiracion = DateTime.UtcNow.AddMinutes(
                double.Parse(jwtConfig["ExpiresInMinutes"]!));

            var token = new JwtSecurityToken(
                issuer:   jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims:   claims,
                expires:  expiracion,
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidarToken(string token)
        {
            var jwtConfig = configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtConfig["Key"]!));

            try
            {
                var handler = new JwtSecurityTokenHandler();
                return handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = key,
                    ValidateIssuer           = true,
                    ValidIssuer              = jwtConfig["Issuer"],
                    ValidateAudience         = true,
                    ValidAudience            = jwtConfig["Audience"],
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero
                }, out _);
            }
            catch { return null; }
        }
    }
}
