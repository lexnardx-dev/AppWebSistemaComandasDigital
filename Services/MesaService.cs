using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Models;
using AppWebSistemaComandasDigital.Repositories;

namespace AppWebSistemaComandasDigital.Services
{
    public class MesaService(IMesaRepository mesaRepository)
    {
        public async Task<IEnumerable<MesaDTO>> GetAllAsync()
        {
            var mesas = await mesaRepository.GetAllAsync();
            return mesas.Select(MapToDTO);
        }

        public async Task<MesaDTO?> GetByIdAsync(int id)
        {
            var mesa = await mesaRepository.GetByIdAsync(id);
            return mesa is null ? null : MapToDTO(mesa);
        }

        public async Task<IEnumerable<MesaDTO>> GetLibresAsync()
        {
            var mesas = await mesaRepository.GetByEstadoAsync(EstadoMesa.Libre);
            return mesas.Select(MapToDTO);
        }

        public async Task<(bool Exitoso, string Mensaje, MesaDTO? Data)> CreateAsync(MesaCreateDTO dto)
        {
            var existe = await mesaRepository.GetByNumeroAsync(dto.Numero);
            if (existe is not null)
                return (false, $"Ya existe una mesa con el número '{dto.Numero}'.", null);

            var mesa = new Mesa
            {
                Numero    = dto.Numero.Trim().ToUpper(),
                Capacidad = dto.Capacidad,
                Estado    = EstadoMesa.Libre
            };

            var creada = await mesaRepository.CreateAsync(mesa);
            return (true, "Mesa creada correctamente.", MapToDTO(creada));
        }

        public async Task<(bool Exitoso, string Mensaje, MesaDTO? Data)> UpdateAsync(int id, MesaUpdateDTO dto)
        {
            var mesa = await mesaRepository.GetByIdAsync(id);
            if (mesa is null)
                return (false, "Mesa no encontrada.", null);

            mesa.Capacidad = dto.Capacidad;
            mesa.Estado    = dto.Estado;

            var actualizada = await mesaRepository.UpdateAsync(mesa);
            return (true, "Mesa actualizada.", MapToDTO(actualizada));
        }

        public async Task<(bool Exitoso, string Mensaje)> DeleteAsync(int id)
        {
            var eliminada = await mesaRepository.DeleteAsync(id);
            return eliminada
                ? (true,  "Mesa eliminada.")
                : (false, "Mesa no encontrada.");
        }

        // ── Mapper ───────────────────────────────────────────────────
        private static MesaDTO MapToDTO(Mesa m) => new()
        {
            Id        = m.Id,
            Numero    = m.Numero,
            Capacidad = m.Capacidad,
            Estado    = m.Estado.ToString()
        };
    }
}
