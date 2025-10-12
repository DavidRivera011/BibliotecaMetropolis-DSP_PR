using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliotecaMetropolis.Models.DB;

/*Integrantes:
 Castellón Hernández, Emily Alessandra
 López Avelar, Vladimir Alexander
 Martínez Nolasco, Julio César
 Peñate Valle, William Eliseo
 Rivera Linares, Julio David
 */

namespace BibliotecaMetropolis.Controllers
{
    public class InstitucionController : Controller
    {
        private readonly BibliotecaMetropolisContext _Context;

        public InstitucionController(BibliotecaMetropolisContext Context)
        {
            _Context = Context;
        }

        public async Task<IActionResult> Index()
        {
            var instituciones = await _Context.Institucions.ToListAsync();
            return View(instituciones);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Institucion institucion)
        {
            if (string.IsNullOrWhiteSpace(institucion.Nombre))
            {
                ModelState.AddModelError("Error", "El nombre de la institución es obligatorio.");
            }

            bool Existe = await _Context.Institucions.AnyAsync(i => i.Nombre == institucion.Nombre);

            if (Existe)
            {
                ModelState.AddModelError("Error", "Esta institución ya está registrada.");
            }
            if (ModelState.IsValid)
            {
                _Context.Add(institucion);
                await _Context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(institucion);
        }

        public async Task<IActionResult> Edit(int id)
        {

            var institucion = await _Context.Institucions.FindAsync(id);
            return institucion == null ? NotFound() : View(institucion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Institucion institucion)
        {
            if (ModelState.IsValid)
            {
                _Context.Update(institucion);
                await _Context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(institucion);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var institucion = await _Context.Institucions.FindAsync(id);

            if (institucion != null)
            {
                _Context.Institucions.Remove(institucion);
                await _Context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
