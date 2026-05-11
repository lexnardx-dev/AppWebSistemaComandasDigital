using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWebSistemaComandasDigital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "SoloAdmin")]
    public class CategoriaController(CategoriaService categoriaService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var categorias = await categoriaService.GetAllAsync();
            return View(categorias);
        }

        [HttpGet]
        public IActionResult Create() => View(new CategoriaCreateDTO());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriaCreateDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var (exitoso, mensaje, _) = await categoriaService.CreateAsync(dto);
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
            var categoria = await categoriaService.GetByIdAsync(id);
            if (categoria is null) return NotFound();

            ViewBag.CategoriaId = id;
            return View(new CategoriaUpdateDTO
            {
                Nombre      = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activa      = categoria.Activa
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoriaUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoriaId = id;
                return View(dto);
            }

            var (exitoso, mensaje, _) = await categoriaService.UpdateAsync(id, dto);
            if (!exitoso)
            {
                ModelState.AddModelError(string.Empty, mensaje);
                ViewBag.CategoriaId = id;
                return View(dto);
            }

            TempData["Exito"] = mensaje;
            return RedirectToAction(nameof(Index));
        }

        // Eliminación inteligente — muestra modal con opciones si tiene platos
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (exitoso, mensaje, tienePlatos) = await categoriaService.DeleteAsync(id);

            if (!exitoso && tienePlatos)
            {
                // Pasar señal a la vista para mostrar modal de opciones
                TempData["ConfirmarEliminacionId"]     = id;
                TempData["ConfirmarEliminacionNombre"] = mensaje;
            }
            else
            {
                TempData[exitoso ? "Exito" : "Error"] = mensaje;
            }

            return RedirectToAction(nameof(Index));
        }

        // Desactivar — oculta del menú pero conserva historial
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            var (exitoso, mensaje) = await categoriaService.DesactivarAsync(id);
            TempData[exitoso ? "Exito" : "Error"] = mensaje;
            return RedirectToAction(nameof(Index));
        }

        // Eliminación forzosa — elimina aunque tenga platos
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceDelete(int id)
        {
            var (exitoso, mensaje) = await categoriaService.ForceDeleteAsync(id);
            TempData[exitoso ? "Exito" : "Error"] = mensaje;
            return RedirectToAction(nameof(Index));
        }
    }
}
