using Microsoft.AspNetCore.Mvc;
using BibliotecaMetropolis.Models.DB;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

/*Integrantes:
 Castellón Hernández, Emily Alessandra
 López Avelar, Vladimir Alexander
 Martínez Nolasco, Julio César
 Peñate Valle, William Eliseo
 Rivera Linares, Julio David
 */

namespace BibliotecaMetropolis.Controllers
{
    public class MaterialController : Controller
    {
        private readonly BibliotecaMetropolisContext _Context;

        public MaterialController(BibliotecaMetropolisContext context)
        {
            _Context = context;
        }

        public async Task<IActionResult> Index()
        {
            var Materiales = await _Context.Materials
                .Include(m => m.IdAutorNavigation)
                .Include(m => m.IdEditorialNavigation)
                .Include(m => m.IdInstitucion).ToListAsync();

            return View(Materiales);
        }

        public IActionResult Create ()
        {
            ViewBag.Autores = new SelectList(_Context.Autors, "IdAutor", "Nombre");
            ViewBag.Editoriales = new SelectList(_Context.Editorials, "IdEditorial", "Nombre");
            ViewBag.Instituciones = new SelectList(_Context.Institucions, "IdInstitucion", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create (Material material)
        {
            if(material.IdAutor == 0)
            {
                ModelState.AddModelError("Error", "El autor no puede ser anónimo. Debe asignarse uno al material.");
            }
            if (material.Tipo.ToLower() == "tesis" && material.IdInstitucion == null)
            {
                ModelState.AddModelError("Error", "Las tesis deben estar asociadas a una institución (Universiidad, Escuela, etc.).");
            }
            if (ModelState.IsValid)
            {
                _Context.Add(material);
                await _Context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Autores = new SelectList(_Context.Autors, "IdAutor", "Nombre", material.IdAutor);
            ViewBag.Editoriales = new SelectList(_Context.Editorials, "IdEditorial", "Nombre", material.IdEditorial);
            ViewBag.Instituciones = new SelectList(_Context.Institucions, "IdInstitucion", "Nombre", material.IdInstitucion);

            return View(material);
        }

        public async Task<IActionResult> Delete (int id)
        {
            var material = await _Context.Materials.FindAsync(id);
            if (material != null)
            {
                _Context.Materials.Remove(material);
                await _Context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
