using BibliotecaMetropolis.Models.DB;
using BibliotecaMetropolis.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

/*Integrantes:
 Castellón Hernández, Emily Alessandra
 López Avelar, Vladimir Alexander
 Martínez Nolasco, Julio César
 Peñate Valle, William Eliseo
 Rivera Linares, Julio David
 */

namespace BibliotecaMetropolis.Controllers
{
    public class AutorController : Controller
    {
        private readonly BibliotecaMetropolisContext _context;

        public AutorController(BibliotecaMetropolisContext context)
        {
            _context = context;
        }

        // GET: Autor
        public async Task<IActionResult> Index()
        {
            var autoresConCount = await Task.Run(() =>
                _context.Autors
                    .Select(a => new
                    {
                        Autor = a,
                        RecursosCount = _context.AutoresRecursos.Count(ar => ar.IdAutor == a.IdAutor)
                    })
                    .OrderBy(x => x.Autor.Nombres)
                    .ThenBy(x => x.Autor.Apellidos)
                    .ToList()
            );

            var model = autoresConCount.Select(x => new Autor
            {
                IdAutor = x.Autor.IdAutor,
                Nombres = x.Autor.Nombres,
                Apellidos = x.Autor.Apellidos,
                RecursosCount = x.RecursosCount
            }).ToList();

            return View(model);
        }


        // GET: Autor/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var autor = await Task.FromResult(_context.Autors.Find(id));
            if (autor == null) return NotFound();
            return View(autor);
        }

        // GET: Autor/Create
        public IActionResult Create() => View();

        // POST: Autor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Autor dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var autor = new Autor
            {
                Nombres = dto.Nombres?.Trim() ?? string.Empty,
                Apellidos = string.IsNullOrWhiteSpace(dto.Apellidos) ? null : dto.Apellidos.Trim()
            };

            _context.Autors.Add(autor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Autor/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var autor = await Task.FromResult(_context.Autors.Find(id));
            if (autor == null) return NotFound();
            return View(autor);
        }

        // POST: Autor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Autor dto)
        {
            if (id != dto.IdAutor) return BadRequest();
            if (!ModelState.IsValid) return View(dto);

            var autor = _context.Autors.Find(id);
            if (autor == null) return NotFound();

            autor.Nombres = dto.Nombres?.Trim() ?? string.Empty;
            autor.Apellidos = string.IsNullOrWhiteSpace(dto.Apellidos) ? null : dto.Apellidos.Trim();

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Autor/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var autor = await Task.FromResult(_context.Autors.Find(id));
            if (autor == null) return NotFound();
            return View(autor);
        }

        // POST: Autor/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var autor = _context.Autors.Find(id);
            if (autor == null) return NotFound();

            var tieneRecursos = _context.AutoresRecursos.Any(ar => ar.IdAutor == id);
            if (tieneRecursos)
            {
                TempData["Error"] = "No se puede eliminar un autor que está vinculado a un recurso.";
                return RedirectToAction(nameof(Index));
            }

            _context.Autors.Remove(autor);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Autor eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
