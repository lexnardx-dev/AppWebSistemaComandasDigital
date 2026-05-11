using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWebSistemaComandasDigital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "SoloAdmin")]
    public class MesaAdminController(MesaService mesaService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var mesas = await mesaService.GetAllAsync();
            return View(mesas);
        }

        [HttpGet]
        public IActionResult Create() => View(new MesaCreateDTO());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MesaCreateDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var (exitoso, mensaje, _) = await mesaService.CreateAsync(dto);
            if (!exitoso)
            {
                ModelState.AddModelError(string.Empty, mensaje);
                return View(dto);
            }

            TempData["Exito"] = mensaje;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var mesa = await mesaService.GetByIdAsync(id);
            if (mesa is null) return NotFound();

            ViewBag.MesaId = id;
            ViewBag.Numero = mesa.Numero;
            return View(new MesaUpdateDTO
            {
                Capacidad = mesa.Capacidad,
                Estado    = Enum.Parse<Models.EstadoMesa>(mesa.Estado)
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MesaUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var mesa = await mesaService.GetByIdAsync(id);
                ViewBag.MesaId = id;
                ViewBag.Numero = mesa?.Numero;
                return View(dto);
            }

            var (exitoso, mensaje, _) = await mesaService.UpdateAsync(id, dto);
            if (!exitoso)
            {
                ModelState.AddModelError(string.Empty, mensaje);
                return View(dto);
            }

            TempData["Exito"] = mensaje;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (exitoso, mensaje) = await mesaService.DeleteAsync(id);
            TempData[exitoso ? "Exito" : "Error"] = mensaje;
            return RedirectToAction(nameof(Index));
        }
    }
}
