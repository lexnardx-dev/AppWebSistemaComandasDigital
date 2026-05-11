using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Helpers;
using AppWebSistemaComandasDigital.Models;
using AppWebSistemaComandasDigital.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace AppWebSistemaComandasDigital.Services
{
    public class AuthService(
        IUsuarioRepository usuarioRepository,
        JwtHelper          jwtHelper,
        IConfiguration     configuration)
    {
        public async Task<(bool Exitoso, string Mensaje, LoginResponseDTO? Data)> LoginAsync(LoginDTO dto)
        {
            var usuario = await usuarioRepository.GetByEmailAsync(dto.Email);
            if (usuario is null)
                return (false, "Credenciales inválidas.", null);

            if (!usuario.Activo)
                return (false, "Usuario inactivo. Contacte al administrador.", null);

            if (!VerificarPassword(dto.Password, usuario.PasswordHash))
                return (false, "Credenciales inválidas.", null);

            var token      = jwtHelper.GenerarToken(usuario);
            var expiracion = DateTime.UtcNow.AddMinutes(
                double.Parse(configuration["Jwt:ExpiresInMinutes"]!));

            var response = new LoginResponseDTO
            {
                Token      = token,
                Nombre     = usuario.Nombre,
                Rol        = usuario.Rol.Nombre,
                Expiracion = expiracion
            };

            return (true, "Login exitoso.", response);
        }

        public async Task<(bool Exitoso, string Mensaje)> RegistrarAsync(
            string nombre, string email, string password, int rolId = 2)
        {
            if (await usuarioRepository.ExisteEmailAsync(email))
                return (false, "El email ya está registrado.");

            var usuario = new Usuario
            {
                Nombre       = nombre.Trim(),
                Email        = email.Trim().ToLower(),
                PasswordHash = HashPassword(password),
                RolId        = rolId,
                Activo       = true,
                FechaCreacion = DateTime.UtcNow
            };

            await usuarioRepository.CreateAsync(usuario);
            return (true, "Usuario registrado correctamente.");
        }

        // ── Crypto ───────────────────────────────────────────────────
        public static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private static bool VerificarPassword(string password, string hash) =>
            HashPassword(password) == hash;
    }
}
