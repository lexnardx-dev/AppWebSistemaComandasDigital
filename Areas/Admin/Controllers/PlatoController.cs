using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Services;
using AppWebSistemaComandasDigital.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace AppWebSistemaComandasDigital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "SoloAdmin")]
    public class PlatoController(
        PlatoService platoService,
        CategoriaService categoriaService,
        ISupabaseStorageService storage,
        IOptions<SupabaseStorageOptions> storageOptions) : Controller
    {
        private async Task CargarCategoriasAsync()
        {
            var categorias = await categoriaService.GetActivasAsync();
            ViewBag.Categorias = new SelectList(categorias, "Id", "Nombre");
        }

        public async Task<IActionResult> Index()
        {
            var platos = await platoService.GetAllAsync();
            return View(platos);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarCategoriasAsync();
            return View(new PlatoCreateDTO { Disponible = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(PlatoCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync();
                return View(dto);
            }

            if (!await ProcesarImagenAsync(dto))
            {
                await CargarCategoriasAsync();
                return View(dto);
            }

            var (exitoso, mensaje, _) = await platoService.CreateAsync(dto);
            if (!exitoso)
            {
                ModelState.AddModelError(string.Empty, mensaje);
                await CargarCategoriasAsync();
                return View(dto);
            }

            TempData["Exito"] = mensaje;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var plato = await platoService.GetByIdAsync(id);
            if (plato is null) return NotFound();

            await CargarCategoriasAsync();

            ViewBag.PlatoId = id;
            return View(new PlatoUpdateDTO
            {
                Nombre      = plato.Nombre,
                Descripcion = plato.Descripcion,
                Precio      = plato.Precio,
                Disponible  = plato.Disponible,
                ImagenUrl   = plato.ImagenUrl,
                CategoriaId = plato.CategoriaId
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, PlatoUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync();
                ViewBag.PlatoId = id;
                return View(dto);
            }

            if (!await ProcesarImagenAsync(dto))
            {
                await CargarCategoriasAsync();
                ViewBag.PlatoId = id;
                return View(dto);
            }

            var (exitoso, mensaje, _) = await platoService.UpdateAsync(id, dto);
            if (!exitoso)
            {
                ModelState.AddModelError(string.Empty, mensaje);
                await CargarCategoriasAsync();
                ViewBag.PlatoId = id;
                return View(dto);
            }

            TempData["Exito"] = mensaje;
            return RedirectToAction(nameof(Index));
        }

        // Eliminación inteligente — muestra modal con opciones si tiene pedidos
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var (exitoso, mensaje, tienePedidos) = await platoService.DeleteAsync(id);

            if (!exitoso && tienePedidos)
            {
                TempData["ConfirmarEliminacionId"]     = id;
                TempData["ConfirmarEliminacionNombre"] = mensaje;
            }
            else
            {
                TempData[exitoso ? "Exito" : "Error"] = mensaje;
            }

            return RedirectToAction(nameof(Index));
        }

        // Marcar como no disponible
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            var (exitoso, mensaje) = await platoService.DesactivarAsync(id);
            TempData[exitoso ? "Exito" : "Error"] = mensaje;
            return RedirectToAction(nameof(Index));
        }

        // Eliminación forzosa
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceDelete(int id)
        {
            var (exitoso, mensaje) = await platoService.ForceDeleteAsync(id);
            TempData[exitoso ? "Exito" : "Error"] = mensaje;
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ProcesarImagenAsync(PlatoCreateDTO dto)
        {
            if (dto.ImagenArchivo is null) return true;

            try
            {
                dto.ImagenUrl = await storage.SubirImagenAsync(
                    dto.ImagenArchivo, storageOptions.Value.Buckets.Platos, HttpContext.RequestAborted);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(dto.ImagenArchivo), ex.Message);
                return false;
            }
        }
    }
}
