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
    public class EditorialController : Controller
    {
       private readonly BibliotecaMetropolisContext _Context;

        public EditorialController(BibliotecaMetropolisContext Context)
        {
            _Context = Context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _Context.Editorials.ToListAsync());
        }

        //Creación/Registro de Editoriales
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Editorial editorial)
        {
            if (ModelState.IsValid)
            {
                _Context.Add(editorial);
                await _Context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(editorial);
        }

        //Edición de Editoriales
        public async Task<IActionResult> Edit(int id)
        {

            var editorial = await _Context.Editorials.FindAsync(id);
            return editorial == null ? NotFound() : View(editorial);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Editorial editorial)
        {
            if (ModelState.IsValid)
            {
                _Context.Update(editorial);
                await _Context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(editorial);
        }

        //Eliminación de Editoriales
        public async Task<IActionResult> Delete(int id)
        { 
            var editorial = await _Context.Editorials.FindAsync(id);
            if (editorial != null)
            {
                _Context.Editorials.Remove(editorial);
                await _Context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
