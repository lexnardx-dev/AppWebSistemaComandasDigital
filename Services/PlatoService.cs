using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Models;
using AppWebSistemaComandasDigital.Repositories;

namespace AppWebSistemaComandasDigital.Services
{
    public class PlatoService(IPlatoRepository platoRepository)
    {
        public async Task<IEnumerable<PlatoDTO>> GetAllAsync()
        {
            var platos = await platoRepository.GetAllAsync();
            return platos.Select(MapToDTO);
        }

        public async Task<PlatoDTO?> GetByIdAsync(int id)
        {
            var plato = await platoRepository.GetByIdAsync(id);
            return plato is null ? null : MapToDTO(plato);
        }

        public async Task<IEnumerable<PlatoDTO>> GetDisponiblesAsync()
        {
            var platos = await platoRepository.GetDisponiblesAsync();
            return platos.Select(MapToDTO);
        }

        public async Task<IEnumerable<PlatoDTO>> GetByCategoriaAsync(int categoriaId)
        {
            var platos = await platoRepository.GetByCategoriaAsync(categoriaId);
            return platos.Select(MapToDTO);
        }

        public async Task<(bool Exitoso, string Mensaje, PlatoDTO? Data)> CreateAsync(PlatoCreateDTO dto)
        {
            if (!PrecioValido(dto.Precio))
                return (false, "El precio debe ser entero o terminar en .50. Ejemplos: 18 o 18.50.", null);

            var plato = new Plato
            {
                Nombre      = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion?.Trim(),
                Precio      = dto.Precio,
                Disponible  = dto.Disponible,
                ImagenUrl   = dto.ImagenUrl?.Trim(),
                CategoriaId = dto.CategoriaId
            };

            var creado    = await platoRepository.CreateAsync(plato);
            var resultado = await platoRepository.GetByIdAsync(creado.Id);
            return (true, "Plato creado correctamente.", MapToDTO(resultado!));
        }

        public async Task<(bool Exitoso, string Mensaje, PlatoDTO? Data)> UpdateAsync(int id, PlatoUpdateDTO dto)
        {
            if (!PrecioValido(dto.Precio))
                return (false, "El precio debe ser entero o terminar en .50. Ejemplos: 18 o 18.50.", null);

            var plato = await platoRepository.GetByIdAsync(id);
            if (plato is null)
                return (false, "Plato no encontrado.", null);

            plato.Nombre      = dto.Nombre.Trim();
            plato.Descripcion = dto.Descripcion?.Trim();
            plato.Precio      = dto.Precio;
            plato.Disponible  = dto.Disponible;
            plato.ImagenUrl   = dto.ImagenUrl?.Trim();
            plato.CategoriaId = dto.CategoriaId;

            var actualizado = await platoRepository.UpdateAsync(plato);
            var resultado   = await platoRepository.GetByIdAsync(actualizado.Id);
            return (true, "Plato actualizado.", MapToDTO(resultado!));
        }

        // Eliminación inteligente — informa si tiene pedidos
        public async Task<(bool Exitoso, string Mensaje, bool TienePedidos)> DeleteAsync(int id)
        {
            var plato = await platoRepository.GetByIdAsync(id);
            if (plato is null)
                return (false, "Plato no encontrado.", false);

            var tienePedidos = await platoRepository.TieneDetallePedidoAsync(id);
            if (tienePedidos)
                return (false, $"El plato '{plato.Nombre}' tiene pedidos registrados.", true);

            await platoRepository.DeleteAsync(id);
            return (true, $"Plato '{plato.Nombre}' eliminado correctamente.", false);
        }

        // Eliminación forzosa — elimina detalles de pedido y luego el plato
        public async Task<(bool Exitoso, string Mensaje)> ForceDeleteAsync(int id)
        {
            var plato = await platoRepository.GetByIdAsync(id);
            if (plato is null)
                return (false, "Plato no encontrado.");

            var nombre = plato.Nombre;
            await platoRepository.ForceDeleteAsync(id);
            return (true, $"Plato '{nombre}' eliminado definitivamente.");
        }

        // Marcar como no disponible
        public async Task<(bool Exitoso, string Mensaje)> DesactivarAsync(int id)
        {
            var plato = await platoRepository.GetByIdAsync(id);
            if (plato is null)
                return (false, "Plato no encontrado.");

            plato.Disponible = false;
            await platoRepository.UpdateAsync(plato);
            return (true, $"Plato '{plato.Nombre}' marcado como no disponible.");
        }

        private static PlatoDTO MapToDTO(Plato p) => new()
        {
            Id              = p.Id,
            Nombre          = p.Nombre,
            Descripcion     = p.Descripcion,
            Precio          = p.Precio,
            Disponible      = p.Disponible,
            ImagenUrl       = p.ImagenUrl,
            CategoriaId     = p.CategoriaId,
            CategoriaNombre = p.Categoria?.Nombre ?? string.Empty
        };

        private static bool PrecioValido(decimal precio) =>
            precio > 0 && precio <= 9999.99m && precio * 2 == Math.Truncate(precio * 2);
    }
}
