using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWebSistemaComandasDigital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "SoloAdmin")]
    public class DashboardController : Controller
    {
        public IActionResult Index() =>
            RedirectToAction("Index", "Home", new { area = "" });
    }
}
