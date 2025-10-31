using BibliotecaMetropolis.Filtros;
using BibliotecaMetropolis.Models.DB;
using Microsoft.AspNetCore.Mvc;
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
    public class EditorialController : Controller
    {
        private readonly BibliotecaMetropolisContext _context;

        public EditorialController(BibliotecaMetropolisContext context)
        {
            _context = context;
        }

        // GET: Editorial
        public async Task<IActionResult> Index()
        {
            var list = await Task.FromResult(_context.Editorials.OrderBy(e => e.Nombre).ToList());
            return View(list);
        }

        // GET: Editorial/Create
        public IActionResult Create() => View();

        // POST: Editorial/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Editorial dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var editorial = new Editorial
            {
                Nombre = dto.Nombre?.Trim() ?? string.Empty,
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim()
            };

            _context.Editorials.Add(editorial);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Editorial/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var editorial = await Task.FromResult(_context.Editorials.Find(id));
            if (editorial == null) return NotFound();
            return View(editorial);
        }

        // POST: Editorial/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Editorial dto)
        {
            if (id != dto.IdEdit) return BadRequest();
            if (!ModelState.IsValid) return View(dto);

            var editorial = _context.Editorials.Find(id);
            if (editorial == null) return NotFound();

            editorial.Nombre = dto.Nombre?.Trim() ?? string.Empty;
            editorial.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Editorial/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var editorial = await Task.FromResult(_context.Editorials.Find(id));
            if (editorial == null) return NotFound();
            return View(editorial);
        }

        // POST: Editorial/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var editorial = _context.Editorials.Find(id);
            if (editorial != null)
            {
                _context.Editorials.Remove(editorial);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Editorial/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var editorial = await Task.FromResult(_context.Editorials.Find(id));
            if (editorial == null) return NotFound();
            return View(editorial);
        }
    }
}
