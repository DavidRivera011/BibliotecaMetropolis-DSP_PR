using BibliotecaMetropolis.Models;
using BibliotecaMetropolis.Models.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

/*Integrantes:
 Castellón Hernández, Emily Alessandra
 López Avelar, Vladimir Alexander
 Martínez Nolasco, Julio César
 Peñate Valle, William Eliseo
 Rivera Linares, Julio David
 */

namespace BibliotecaMetropolis.Controllers
{
    public class AuthController : Controller
    {
        private readonly BibliotecaMetropolisContext _context;
        private readonly IConfiguration _config;

        public AuthController(BibliotecaMetropolisContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // LOGIN

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.NombreUsuario == model.NombreUsuario && u.Activo);

            if (user == null)
            {
                ModelState.AddModelError("", "Usuario no encontrado o inactivo.");
                return View(model);
            }

            // BCrypt para verificar la contraseña (HASH)
            bool validPassword = BCrypt.Net.BCrypt.Verify(model.Contrasena, user.Contrasena);
            if (!validPassword)
            {
                ModelState.AddModelError("", "Contraseña incorrecta.");
                return View(model);
            }

            // Generar JWT
            var token = GenerarJwt(user);

            // Guardar token en Session junto a usuario y rol
            HttpContext.Session.SetString("JWToken", token);
            HttpContext.Session.SetString("NombreUsuario", user.NombreUsuario);
            HttpContext.Session.SetString("Rol", user.IdRolNavigation?.NombreRol ?? string.Empty);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Generación del token
        private string GenerarJwt(Usuario usuario)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var keyString = jwtSettings["Key"];
            if (string.IsNullOrEmpty(keyString))
                throw new InvalidOperationException("La clave JWT no está configurada.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var nombreRol = usuario.IdRolNavigation?.NombreRol ?? string.Empty;

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.NombreUsuario),
                new Claim("rol", nombreRol),
                new Claim("id", usuario.IdUsuario.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
