using AppWebSistemaComandasDigital.Dtos;
using AppWebSistemaComandasDigital.Helpers;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppWebSistemaComandasDigital.Controllers.API
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController(AuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseHelper.Error<object>("Datos inválidos.",
                    ModelState.Values.SelectMany(v => v.Errors)
                              .Select(e => e.ErrorMessage).ToList()));

            var (exitoso, mensaje, data) = await authService.LoginAsync(dto);

            if (!exitoso)
                return Unauthorized(ResponseHelper.Error<object>(mensaje));

            // Guardar token en cookie HttpOnly para MVC
            Response.Cookies.Append("jwt", data!.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure   = true,
                SameSite = SameSiteMode.Strict,
                Expires  = data.Expiracion
            });

            return Ok(ResponseHelper.Ok(data, mensaje));
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok(ResponseHelper.Ok<object>(null!, "Sesión cerrada."));
        }
    }
}
