using BibliotecaMetropolis.Models;
using BibliotecaMetropolis.Models.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace BibliotecaMetropolis.Controllers
{
    public class AuthController : Controller
    {
        private readonly BibliotecaMetropolisContext _context;
        private readonly IConfiguration _config;

        public AuthController(BibliotecaMetropolisContext context, IConfiguration config)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // LOGIN

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Usuario model)
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

            if (string.IsNullOrEmpty(user.Contrasena))
            {
                ModelState.AddModelError("", "Contraseña no configurada para el usuario.");
                return View(model);
            }

            // BCrypt para verificar la contraseña (HASH)
            bool validPassword = false;
            try
            {
                validPassword = BCrypt.Net.BCrypt.Verify(model.Contrasena, user.Contrasena);
            }
            catch
            {
                // en caso de hash inválido o formato inesperado
                validPassword = false;
            }

            if (!validPassword)
            {
                ModelState.AddModelError("", "Contraseña incorrecta.");
                return View(model);
            }

            // Generar JWT
            var token = GenerarJwt(user);

            // Guardar token en Session junto a usuario y rol
            HttpContext.Session.SetString("JWToken", token);
            HttpContext.Session.SetString("NombreUsuario", user.NombreUsuario ?? string.Empty);
            HttpContext.Session.SetString("Rol", user.IdRolNavigation?.NombreRol ?? string.Empty);

            return RedirectToAction("Index", "Home");
        }

        // LOGOUT
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
                new Claim(ClaimTypes.Name, usuario.NombreUsuario ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Role, nombreRol)
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
