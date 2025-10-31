using BibliotecaMetropolis.Models.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotecaMetropolis.Filtros;

/*Integrantes:
 Castellón Hernández, Emily Alessandra
 López Avelar, Vladimir Alexander
 Martínez Nolasco, Julio César
 Peñate Valle, William Eliseo
 Rivera Linares, Julio David
 */

namespace BibliotecaMetropolis.Controllers
{
    [RoleAuthorize("Administrador")]
    public class UsuariosController : Controller
    {
        private readonly BibliotecaMetropolisContext _context;

        public UsuariosController(BibliotecaMetropolisContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            ViewData["Roles"] = await _context.Rols.OrderBy(r => r.NombreRol).ToListAsync();

            var usuarios = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();

            return View(usuarios);
        }

        // GET: Usuarios/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Roles"] = await _context.Rols.OrderBy(r => r.NombreRol).ToListAsync();
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string NombreUsuario, string Contrasena, string? NombreCompleto, bool Activo = true, int IdRol = 0)
        {
            ModelState.Remove(nameof(NombreUsuario));
            ModelState.Remove(nameof(Contrasena));
            ModelState.Remove(nameof(IdRol));

            // validación inicial
            if (string.IsNullOrWhiteSpace(NombreUsuario))
                ModelState.AddModelError(nameof(NombreUsuario), "El nombre de usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(Contrasena) || Contrasena.Length < 6)
                ModelState.AddModelError(nameof(Contrasena), "La contraseña es obligatoria (mínimo 6 caracteres).");

            if (!string.IsNullOrWhiteSpace(NombreUsuario))
            {
                var exists = await _context.Usuarios.AnyAsync(u => u.NombreUsuario == NombreUsuario);
                if (exists) ModelState.AddModelError(nameof(NombreUsuario), "El nombre de usuario ya existe.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Roles"] = await _context.Rols.OrderBy(r => r.NombreRol).ToListAsync();

                var errors = ModelState
                    .Where(kvp => kvp.Value != null && kvp.Value.Errors != null && kvp.Value.Errors.Any())
                    .SelectMany(kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                TempData["Error"] = string.Join(" - ", errors);

                var vm = new Usuario
                {
                    NombreUsuario = NombreUsuario,
                    NombreCompleto = NombreCompleto,
                    Activo = Activo,
                    IdRol = IdRol
                };

                return View(vm);
            }

            // crear y guardar
            var usuario = new Usuario
            {
                NombreUsuario = NombreUsuario.Trim(),
                NombreCompleto = string.IsNullOrWhiteSpace(NombreCompleto) ? null : NombreCompleto.Trim(),
                Activo = Activo,
                IdRol = IdRol,
                Contrasena = BCrypt.Net.BCrypt.HashPassword(Contrasena)
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Usuario creado correctamente.";
            return RedirectToAction(nameof(Index));
        }


        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            ViewData["Roles"] = await _context.Rols.OrderBy(r => r.NombreRol).ToListAsync();
            return View(usuario);
        }

        // POST: Usuarios/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int IdUsuario, string NombreUsuario, string? NombreCompleto, bool Activo, int IdRol, string? NewPassword)
        {
            if (IdUsuario <= 0) return BadRequest();

            var usuario = await _context.Usuarios.FindAsync(IdUsuario);
            if (usuario == null) return NotFound();

            if (string.IsNullOrWhiteSpace(NombreUsuario))
                ModelState.AddModelError(nameof(NombreUsuario), "El nombre de usuario es obligatorio.");

            if (!string.IsNullOrEmpty(NewPassword) && NewPassword.Length < 6)
                ModelState.AddModelError("NewPassword", "La nueva contraseña debe tener al menos 6 caracteres.");

            if (IdRol <= 0)
                ModelState.AddModelError(nameof(IdRol), "Selecciona un rol válido.");
            else
            {
                var rolExists = await _context.Rols.AnyAsync(r => r.IdRol == IdRol);
                if (!rolExists) ModelState.AddModelError(nameof(IdRol), "Rol inválido.");
            }

            if (!string.Equals(usuario.NombreUsuario, NombreUsuario, System.StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _context.Usuarios.AnyAsync(u => u.NombreUsuario == NombreUsuario && u.IdUsuario != IdUsuario);
                if (exists) ModelState.AddModelError(nameof(NombreUsuario), "El nombre de usuario ya está en uso.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Roles"] = await _context.Rols.OrderBy(r => r.NombreRol).ToListAsync();

                var errors = ModelState
                    .Where(kvp => kvp.Value != null && kvp.Value.Errors != null && kvp.Value.Errors.Any())
                    .SelectMany(kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                TempData["Error"] = string.Join(" - ", errors);

                var vm = new Usuario
                {
                    IdUsuario = IdUsuario,
                    NombreUsuario = NombreUsuario,
                    NombreCompleto = NombreCompleto,
                    Activo = Activo,
                    IdRol = IdRol
                };

                return View(vm);
            }

            usuario.NombreUsuario = NombreUsuario.Trim();
            usuario.NombreCompleto = string.IsNullOrWhiteSpace(NombreCompleto) ? null : NombreCompleto.Trim();
            usuario.Activo = Activo;
            usuario.IdRol = IdRol;

            if (!string.IsNullOrEmpty(NewPassword))
            {
                usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Usuarios/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Usuario eliminado correctamente.";
            return RedirectToAction("Index", "Usuarios");
        }
    }
}
