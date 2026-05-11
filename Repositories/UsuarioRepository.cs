using AppWebSistemaComandasDigital.Data;
using AppWebSistemaComandasDigital.Models;
using Microsoft.EntityFrameworkCore;

namespace AppWebSistemaComandasDigital.Repositories
{
    public class UsuarioRepository(ApplicationDbContext context) : IUsuarioRepository
    {
        public async Task<IEnumerable<Usuario>> GetAllAsync() =>
            await context.Usuarios.AsNoTracking()
                .Include(u => u.Rol)
                .ToListAsync();

        public async Task<Usuario?> GetByIdAsync(int id) =>
            await context.Usuarios.AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);

        public async Task<Usuario?> GetByEmailAsync(string email) =>
            await context.Usuarios.AsNoTracking()
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == email);

        public async Task<bool> ExisteEmailAsync(string email) =>
            await context.Usuarios.AnyAsync(u => u.Email == email);

        public async Task<Usuario> CreateAsync(Usuario usuario)
        {
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuario> UpdateAsync(Usuario usuario)
        {
            var existente = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuario.Id);
            if (existente is null)
                throw new InvalidOperationException("Usuario no encontrado.");

            existente.Nombre = usuario.Nombre;
            existente.Email = usuario.Email;
            existente.PasswordHash = usuario.PasswordHash;
            existente.Activo = usuario.Activo;
            existente.RolId = usuario.RolId;

            await context.SaveChangesAsync();
            return existente;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await context.Usuarios.FindAsync(id);
            if (usuario is null) return false;
            context.Usuarios.Remove(usuario);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
