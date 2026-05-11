using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Helpers;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppWebSistemaComandasDigital.Controllers.API
{
    [ApiController]
    [Route("api/platos")]
    public class PlatoApiController(PlatoService platoService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(ResponseHelper.Ok(await platoService.GetAllAsync()));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var plato = await platoService.GetByIdAsync(id);
            return plato is null
                ? NotFound(ResponseHelper.NoEncontrado<object>("Plato"))
                : Ok(ResponseHelper.Ok(plato));
        }

        [HttpGet("disponibles")]
        public async Task<IActionResult> GetDisponibles() =>
            Ok(ResponseHelper.Ok(await platoService.GetDisponiblesAsync()));

        [HttpGet("categoria/{categoriaId:int}")]
        public async Task<IActionResult> GetByCategoria(int categoriaId) =>
            Ok(ResponseHelper.Ok(await platoService.GetByCategoriaAsync(categoriaId)));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PlatoCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseHelper.Error<object>("Datos inválidos."));

            var (exitoso, mensaje, data) = await platoService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = data!.Id },
                ResponseHelper.Creado(data, mensaje));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] PlatoUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseHelper.Error<object>("Datos inválidos."));

            var (exitoso, mensaje, data) = await platoService.UpdateAsync(id, dto);
            if (!exitoso) return NotFound(ResponseHelper.Error<object>(mensaje));
            return Ok(ResponseHelper.Ok(data, mensaje));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            // DeleteAsync ahora devuelve 3 valores — descartamos el tercero con _
            var (exitoso, mensaje, _) = await platoService.DeleteAsync(id);
            return exitoso
                ? Ok(ResponseHelper.Ok<object>(null!, mensaje))
                : Conflict(ResponseHelper.Error<object>(mensaje));
        }

        [HttpDelete("{id:int}/force")]
        public async Task<IActionResult> ForceDelete(int id)
        {
            var (exitoso, mensaje) = await platoService.ForceDeleteAsync(id);
            return exitoso
                ? Ok(ResponseHelper.Ok<object>(null!, mensaje))
                : NotFound(ResponseHelper.Error<object>(mensaje));
        }

        [HttpPatch("{id:int}/desactivar")]
        public async Task<IActionResult> Desactivar(int id)
        {
            var (exitoso, mensaje) = await platoService.DesactivarAsync(id);
            return exitoso
                ? Ok(ResponseHelper.Ok<object>(null!, mensaje))
                : NotFound(ResponseHelper.Error<object>(mensaje));
        }
    }
}
