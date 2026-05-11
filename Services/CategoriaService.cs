using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Models;
using AppWebSistemaComandasDigital.Repositories;

namespace AppWebSistemaComandasDigital.Services
{
    public class CategoriaService(ICategoriaRepository categoriaRepository)
    {
        public async Task<IEnumerable<CategoriaDTO>> GetAllAsync()
        {
            var categorias = await categoriaRepository.GetAllAsync();
            return categorias.Select(MapToDTO);
        }

        public async Task<CategoriaDTO?> GetByIdAsync(int id)
        {
            var categoria = await categoriaRepository.GetByIdAsync(id);
            return categoria is null ? null : MapToDTO(categoria);
        }

        public async Task<IEnumerable<CategoriaDTO>> GetActivasAsync()
        {
            var categorias = await categoriaRepository.GetActivasAsync();
            return categorias.Select(MapToDTO);
        }

        public async Task<(bool Exitoso, string Mensaje, CategoriaDTO? Data)> CreateAsync(CategoriaCreateDTO dto)
        {
            var existe = await categoriaRepository.GetByNombreAsync(dto.Nombre);
            if (existe is not null)
                return (false, $"Ya existe una categoría con el nombre '{dto.Nombre}'.", null);

            var categoria = new Categoria
            {
                Nombre      = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion?.Trim(),
                Activa      = true
            };

            var creada = await categoriaRepository.CreateAsync(categoria);
            return (true, "Categoría creada correctamente.", MapToDTO(creada));
        }

        public async Task<(bool Exitoso, string Mensaje, CategoriaDTO? Data)> UpdateAsync(int id, CategoriaUpdateDTO dto)
        {
            var categoria = await categoriaRepository.GetByIdAsync(id);
            if (categoria is null)
                return (false, "Categoría no encontrada.", null);

            var duplicado = await categoriaRepository.GetByNombreAsync(dto.Nombre);
            if (duplicado is not null && duplicado.Id != id)
                return (false, $"Ya existe otra categoría con el nombre '{dto.Nombre}'.", null);

            categoria.Nombre      = dto.Nombre.Trim();
            categoria.Descripcion = dto.Descripcion?.Trim();
            categoria.Activa      = dto.Activa;

            var actualizada = await categoriaRepository.UpdateAsync(categoria);
            return (true, "Categoría actualizada.", MapToDTO(actualizada));
        }

        // Eliminación inteligente — informa si tiene platos
        public async Task<(bool Exitoso, string Mensaje, bool TienePlatos)> DeleteAsync(int id)
        {
            var categoria = await categoriaRepository.GetByIdAsync(id);
            if (categoria is null)
                return (false, "Categoría no encontrada.", false);

            var tienePlatos = await categoriaRepository.TienePlatosAsync(id);
            if (tienePlatos)
                return (false, $"La categoría '{categoria.Nombre}' tiene platos registrados.", true);

            await categoriaRepository.DeleteAsync(id);
            return (true, $"Categoría '{categoria.Nombre}' eliminada correctamente.", false);
        }

        // Eliminación forzosa — elimina categoría + platos + detalles en transacción
        public async Task<(bool Exitoso, string Mensaje)> ForceDeleteAsync(int id)
        {
            var categoria = await categoriaRepository.GetByIdAsync(id);
            if (categoria is null)
                return (false, "Categoría no encontrada.");

            var nombre = categoria.Nombre;
            await categoriaRepository.ForceDeleteAsync(id);
            return (true, $"Categoría '{nombre}' y sus platos eliminados definitivamente.");
        }

        // Desactivar — oculta del menú pero conserva historial
        public async Task<(bool Exitoso, string Mensaje)> DesactivarAsync(int id)
        {
            var categoria = await categoriaRepository.GetByIdAsync(id);
            if (categoria is null)
                return (false, "Categoría no encontrada.");

            categoria.Activa = false;
            await categoriaRepository.UpdateAsync(categoria);
            return (true, $"Categoría '{categoria.Nombre}' marcada como inactiva.");
        }

        private static CategoriaDTO MapToDTO(Categoria c) => new()
        {
            Id          = c.Id,
            Nombre      = c.Nombre,
            Descripcion = c.Descripcion,
            Activa      = c.Activa
        };
    }
}
