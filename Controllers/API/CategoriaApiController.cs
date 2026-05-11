using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Helpers;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppWebSistemaComandasDigital.Controllers.API
{
    [ApiController]
    [Route("api/categorias")]
    public class CategoriaApiController(CategoriaService categoriaService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(ResponseHelper.Ok(await categoriaService.GetAllAsync()));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var categoria = await categoriaService.GetByIdAsync(id);
            return categoria is null
                ? NotFound(ResponseHelper.NoEncontrado<object>("Categoría"))
                : Ok(ResponseHelper.Ok(categoria));
        }

        [HttpGet("activas")]
        public async Task<IActionResult> GetActivas() =>
            Ok(ResponseHelper.Ok(await categoriaService.GetActivasAsync()));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoriaCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseHelper.Error<object>("Datos inválidos."));

            var (exitoso, mensaje, data) = await categoriaService.CreateAsync(dto);
            if (!exitoso) return Conflict(ResponseHelper.Error<object>(mensaje));

            return CreatedAtAction(nameof(GetById), new { id = data!.Id },
                ResponseHelper.Creado(data, mensaje));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoriaUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseHelper.Error<object>("Datos inválidos."));

            var (exitoso, mensaje, data) = await categoriaService.UpdateAsync(id, dto);
            if (!exitoso) return NotFound(ResponseHelper.Error<object>(mensaje));
            return Ok(ResponseHelper.Ok(data, mensaje));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            // DeleteAsync ahora devuelve 3 valores — descartamos el tercero con _
            var (exitoso, mensaje, _) = await categoriaService.DeleteAsync(id);
            return exitoso
                ? Ok(ResponseHelper.Ok<object>(null!, mensaje))
                : Conflict(ResponseHelper.Error<object>(mensaje));
        }

        [HttpDelete("{id:int}/force")]
        public async Task<IActionResult> ForceDelete(int id)
        {
            var (exitoso, mensaje) = await categoriaService.ForceDeleteAsync(id);
            return exitoso
                ? Ok(ResponseHelper.Ok<object>(null!, mensaje))
                : NotFound(ResponseHelper.Error<object>(mensaje));
        }

        [HttpPatch("{id:int}/desactivar")]
        public async Task<IActionResult> Desactivar(int id)
        {
            var (exitoso, mensaje) = await categoriaService.DesactivarAsync(id);
            return exitoso
                ? Ok(ResponseHelper.Ok<object>(null!, mensaje))
                : NotFound(ResponseHelper.Error<object>(mensaje));
        }
    }
}
