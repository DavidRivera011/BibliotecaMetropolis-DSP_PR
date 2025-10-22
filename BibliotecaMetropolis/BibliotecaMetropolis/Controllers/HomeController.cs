using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace BibliotecaMetropolis.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
            {
                // No hay token => redirige a Login
                return RedirectToAction("Login", "Auth");
            }

            // Verificamos si el token ya expiró (exp viene en formato UTC)
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            if (jwt.ValidTo < DateTime.UtcNow)
            {
                // Token expirado => limpiar sesión y redirigir
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            // Si todo está bien, pasamos el usuario a la vista
            ViewBag.Usuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.Rol = HttpContext.Session.GetString("Rol");

            return View();
        }
    }
}
