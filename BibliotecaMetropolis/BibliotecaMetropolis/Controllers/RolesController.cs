using BibliotecaMetropolis.Filtros;
using BibliotecaMetropolis.Models.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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
    public class RolesController : Controller
    {
        private readonly BibliotecaMetropolisContext _context;

        public RolesController(BibliotecaMetropolisContext context)
        {
            _context = context;
        }

        // GET: Roles (lista compacta)
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Rols.OrderBy(r => r.NombreRol).ToListAsync();
            return View(roles);
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Rol dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NombreRol))
            {
                ModelState.AddModelError(nameof(dto.NombreRol), "El nombre del rol es obligatorio.");
            }

            var exists = await _context.Rols.AnyAsync(r => r.NombreRol == dto.NombreRol);
            if (exists) ModelState.AddModelError(nameof(dto.NombreRol), "El rol ya existe.");

            if (!ModelState.IsValid)
            {
                var roles = await _context.Rols.OrderBy(r => r.NombreRol).ToListAsync();
                return View("Index", roles);
            }

            var rol = new Rol { NombreRol = dto.NombreRol.Trim() };
            _context.Rols.Add(rol);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Usuarios");
        }

        // POST: Roles/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var rol = await _context.Rols.FindAsync(id);
            if (rol == null) return NotFound();

            var linked = await _context.Usuarios.AnyAsync(u => u.IdRol == id);
            if (linked)
            {
                TempData["Error"] = "No se puede eliminar el rol porque tiene usuarios asignados.";
                return RedirectToAction("Index", "Usuarios");
            }

            _context.Rols.Remove(rol);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Rol eliminado correctamente.";
            return RedirectToAction("Index", "Usuarios");
        }
    }
}
