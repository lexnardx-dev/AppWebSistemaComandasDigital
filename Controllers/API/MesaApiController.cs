using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Helpers;
using AppWebSistemaComandasDigital.RealTime;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AppWebSistemaComandasDigital.Controllers.API
{
    [ApiController]
    [Route("api/mesas")]
    public class MesaApiController(
        MesaService            mesaService,
        IHubContext<PedidoHub> hub) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(ResponseHelper.Ok(await mesaService.GetAllAsync()));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var mesa = await mesaService.GetByIdAsync(id);
            return mesa is null
                ? NotFound(ResponseHelper.NoEncontrado<object>("Mesa"))
                : Ok(ResponseHelper.Ok(mesa));
        }

        [HttpGet("libres")]
        public async Task<IActionResult> GetLibres() =>
            Ok(ResponseHelper.Ok(await mesaService.GetLibresAsync()));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MesaCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseHelper.Error<object>("Datos inválidos."));

            var (exitoso, mensaje, data) = await mesaService.CreateAsync(dto);
            if (!exitoso) return Conflict(ResponseHelper.Error<object>(mensaje));

            await PedidoHubNotificaciones.MesaActualizada(hub, data!.Id, data.Estado);
            return CreatedAtAction(nameof(GetById), new { id = data.Id },
                ResponseHelper.Creado(data, mensaje));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] MesaUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseHelper.Error<object>("Datos inválidos."));

            var (exitoso, mensaje, data) = await mesaService.UpdateAsync(id, dto);
            if (!exitoso) return NotFound(ResponseHelper.Error<object>(mensaje));

            await PedidoHubNotificaciones.MesaActualizada(hub, id, data!.Estado);
            return Ok(ResponseHelper.Ok(data, mensaje));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (exitoso, mensaje) = await mesaService.DeleteAsync(id);
            return exitoso
                ? Ok(ResponseHelper.Ok<object>(null!, mensaje))
                : NotFound(ResponseHelper.Error<object>(mensaje));
        }
    }
}
