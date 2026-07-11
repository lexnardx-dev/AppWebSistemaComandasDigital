using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Helpers;
using AppWebSistemaComandasDigital.Models;
using AppWebSistemaComandasDigital.RealTime;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AppWebSistemaComandasDigital.Controllers.API
{
    [ApiController]
    [Route("api/pedidos")]
    public class PedidoApiController(
        PedidoService          pedidoService,
        NotificacionService    notificacionService,
        IHubContext<PedidoHub> hub) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(ResponseHelper.Ok(await pedidoService.GetAllAsync()));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pedido = await pedidoService.GetByIdAsync(id);
            return pedido is null
                ? NotFound(ResponseHelper.NoEncontrado<object>("Pedido"))
                : Ok(ResponseHelper.Ok(pedido));
        }

        [HttpGet("estado/{estado}")]
        public async Task<IActionResult> GetByEstado(string estado)
        {
            if (!Enum.TryParse<EstadoPedido>(estado, true, out var estadoEnum))
                return BadRequest(ResponseHelper.Error<object>("Estado inválido."));

            return Ok(ResponseHelper.Ok(await pedidoService.GetByEstadoAsync(estadoEnum)));
        }

        [HttpGet("mesa/{mesaId:int}")]
        public async Task<IActionResult> GetByMesa(int mesaId) =>
            Ok(ResponseHelper.Ok(await pedidoService.GetByMesaAsync(mesaId)));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PedidoCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseHelper.Error<object>("Datos inválidos."));

            var usuarioIdClaim = User.FindFirstValue("usuario_id");
            if (!int.TryParse(usuarioIdClaim, out var usuarioId) || usuarioId == 0)
                return Unauthorized(ResponseHelper.NoAutorizado<object>());

            var (exitoso, mensaje, data) = await pedidoService.CreateAsync(dto, usuarioId);
            if (!exitoso) return BadRequest(ResponseHelper.Error<object>(mensaje));

            await notificacionService.CrearParaRolesAsync(
                ["Admin", "Cocina"],
                TiposNotificacion.Pedido,
                $"Nuevo pedido #{data!.Id}",
                $"Mesa {data.MesaNumero} enviada a cocina por {data.UsuarioNombre}.",
                PrioridadNotificacion.Urgente,
                $"/Pedido/Detalle/{data.Id}");

            await PedidoHubNotificaciones.NuevoPedido(hub, data!);

            return CreatedAtAction(nameof(GetById), new { id = data!.Id },
                ResponseHelper.Creado(data, mensaje));
        }

        [HttpPatch("{id:int}/estado")]
        public async Task<IActionResult> UpdateEstado(int id, [FromBody] PedidoUpdateEstadoDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseHelper.Error<object>("Datos inválidos."));

            // Validar que el string sea un EstadoPedido válido
            if (!Enum.TryParse<EstadoPedido>(dto.Estado, ignoreCase: true, out _))
                return BadRequest(ResponseHelper.Error<object>(
                    $"Estado '{dto.Estado}' inválido. Valores válidos: Pendiente, EnCocina, Listo, Entregado, Cancelado."));

            var (exitoso, mensaje) = await pedidoService.UpdateEstadoAsync(id, dto);
            if (!exitoso) return NotFound(ResponseHelper.Error<object>(mensaje));

            var pedido = await pedidoService.GetByIdAsync(id);
            await notificacionService.CrearParaRolesAsync(
                ["Admin", "Cocina"],
                TiposNotificacion.EstadoPedido,
                $"Pedido #{id} actualizado",
                pedido is null
                    ? $"Nuevo estado: {dto.Estado}."
                    : $"Mesa {pedido.MesaNumero} ahora esta en estado {dto.Estado}.",
                dto.EstadoEnum is EstadoPedido.Cancelado ? PrioridadNotificacion.Importante : PrioridadNotificacion.Normal,
                $"/Pedido/Detalle/{id}");

            await PedidoHubNotificaciones.EstadoActualizado(hub, id, dto.Estado);

            return Ok(ResponseHelper.Ok<object>(null!, mensaje));
        }
    }
}
