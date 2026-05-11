using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppWebSistemaComandasDigital.Controllers
{
    [Authorize]
    public class NotificacionesController : Controller
    {
        public IActionResult Index() => View();
    }
}
