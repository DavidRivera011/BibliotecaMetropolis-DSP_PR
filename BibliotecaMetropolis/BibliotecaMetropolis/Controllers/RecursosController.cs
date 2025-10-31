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

        // GET 
        public RecursosController(BibliotecaMetropolisContext context)
        {
            _context = context;
            _config = config;
        }

        // GET /Details/{id}
        [RoleAuthorize("Administrador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var recurso = await _context.Recursos
                .Include(r => r.IdEditNavigation)
                .Include(r => r.IdPaisNavigation)
                .Include(r => r.IdTipoRNavigation)
                .Include(r => r.IdPalabraClaves)
                .FirstOrDefaultAsync(r => r.IdRec == id);

            if (recurso == null) return NotFound();

            var autores = await _context.AutoresRecursos
                .Include(ar => ar.IdAutorNavigation)
                .Where(ar => ar.IdRec == id)
                .OrderByDescending(ar => ar.EsPrincipal)
                .ToListAsync();

            var autoresStr = autores.Any()
                ? string.Join(", ", autores.Select(a => $"{a.IdAutorNavigation?.Nombres} {a.IdAutorNavigation?.Apellidos}".Trim()))
                : "Desconocido";

            var vm = new RecursoDetailsViewModel
            {
                IdRec = recurso.IdRec,
                Titulo = recurso.Titulo,
                ImagenRuta = recurso.ImagenRuta,
                Autores = autoresStr,
                Editorial = recurso.IdEditNavigation?.Nombre,
                AnioPublicacion = recurso.AnioPublicacion,
                Edicion = recurso.Edicion,
                Cantidad = recurso.Cantidad ?? 0,
                TipoRecurso = recurso.IdTipoRNavigation?.Nombre,
                Pais = recurso.IdPaisNavigation?.Nombre,
                Precio = recurso.Precio,
                PalabrasBusqueda = recurso.PalabrasBusqueda,
                Descripcion = recurso.Descripcion
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST CREATE
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
        // GET EDIT
        [RoleAuthorize("Administrador")]
        public async Task<IActionResult> Edit(int id)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }


        // POST EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RecursoEditViewModel model)
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
                var errors = ModelState
                    .Where(kvp => kvp.Value != null && kvp.Value.Errors != null && kvp.Value.Errors.Any())
                    .SelectMany(kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                TempData["Error"] = string.Join(" - ", errors);

                ViewData["Tipos"] = await _context.TipoRecursos.OrderBy(t => t.Nombre).ToListAsync();
                ViewData["Paises"] = await _context.Pais.OrderBy(p => p.Nombre).ToListAsync();
                ViewData["Editoriales"] = await _context.Editorials.OrderBy(e => e.Nombre).ToListAsync();
                ViewData["Autores"] = await _context.Autors.OrderBy(a => a.Nombres).ThenBy(a => a.Apellidos).ToListAsync();
                return View(model);
            }

            var recurso = await _context.Recursos
                .Include(r => r.IdPalabraClaves)
                .FirstOrDefaultAsync(r => r.IdRec == model.IdRec);

            if (recurso == null) return NotFound();

            recurso.ImagenRuta = string.IsNullOrWhiteSpace(model.ImagenRuta) ? null : model.ImagenRuta.Trim();
            recurso.Titulo = model.Titulo?.Trim() ?? string.Empty;
            recurso.Descripcion = string.IsNullOrWhiteSpace(model.Descripcion) ? null : model.Descripcion.Trim();
            recurso.AnioPublicacion = model.AnioPublicacion;
            recurso.Edicion = string.IsNullOrWhiteSpace(model.Edicion) ? null : model.Edicion.Trim();
            recurso.Cantidad = model.Cantidad;
            recurso.IdTipoR = model.IdTipoR;
            recurso.IdPais = model.IdPais;
            recurso.Precio = model.Precio;
            recurso.IdEdit = model.IdEdit;

            var tags = keywords;
            recurso.PalabrasBusqueda = tags.Any() ? string.Join(", ", tags) : null;

            var normalized = tags.Select(t => t.ToLowerInvariant()).ToList();
            var existingPalabras = new List<PalabraClave>();
            if (normalized.Any())
            {
                existingPalabras = await _context.PalabraClaves
                    .Where(p => normalized.Contains(p.Palabra.ToLower()))
                    .ToListAsync();
            }

            recurso.IdPalabraClaves.Clear();
            foreach (var tag in tags)
            {
                var tagLower = tag.ToLowerInvariant();
                var palabra = existingPalabras.FirstOrDefault(p => p.Palabra.ToLower() == tagLower);
                if (palabra == null)
                {
                    palabra = new PalabraClave { Palabra = tag };
                    _context.PalabraClaves.Add(palabra);
                    existingPalabras.Add(palabra);
                }
                recurso.IdPalabraClaves.Add(palabra);
            }

            var currentARs = await _context.AutoresRecursos
                .Where(ar => ar.IdRec == recurso.IdRec)
                .ToListAsync();

            _context.AutoresRecursos.RemoveRange(currentARs);

            for (int i = 0; i < orderedSelected.Count; i++)
            {
                var idAutor = orderedSelected[i];
                _context.AutoresRecursos.Add(new AutoresRecurso
                {
                    IdRec = recurso.IdRec,
                    IdAutor = idAutor,
                    EsPrincipal = (i == 0)
                });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecursoExists(recurso.IdRec)) return NotFound();
                throw;
            }

            return RedirectToAction("Details", new { id = model.IdRec });
        }

        // POST DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            var recurso = await _context.Recursos.FindAsync(id);
            if (recurso == null) return NotFound();

            if ((recurso.Cantidad ?? 0) > 0)
            {
                TempData["Error"] = "El recurso no puede eliminarse porque su cantidad es mayor a 0.";
                return RedirectToAction(nameof(Details), new { id });
            }

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
