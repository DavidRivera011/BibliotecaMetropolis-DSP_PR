using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
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
    public class AutorController : Controller
    {
        private readonly BibliotecaMetropolisContext _Context;

        public AutorController(BibliotecaMetropolisContext Context)
        {
            _Context = Context;
        }

        public async Task<IActionResult> Index()
        {
            var autores = await _Context.Autors.ToListAsync();
            return View(autores);
        }

        //Creación/Registro de Autores
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Autor autor)
        {
            if(string.IsNullOrWhiteSpace(autor.Nombre) || string.IsNullOrWhiteSpace(autor.Apellido))
            {
                ModelState.AddModelError("Error", "El nombre y apellido del autor son obligatorios.");
            }

            bool Existe = await _Context.Autors.AnyAsync(a => a.Nombre == autor.Nombre && a.Apellido == autor.Apellido);

            if (Existe) 
            {
                ModelState.AddModelError("Error", "Este autor ya está registrado.");
            }

            if(ModelState.IsValid)
            {
                _Context.Add(autor);
                await _Context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(autor);
        }

        //Edición de Autores
        public async Task<IActionResult> Edit(Autor autor)
        {
            if(ModelState.IsValid)
            {
                _Context.Update(autor);
                await _Context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(autor);
        }


        //Eliminación de Autores
        public async Task<IActionResult> Delete(int id)
        {
            var autor = await _Context.Autors.FindAsync(id);

            if (autor != null)
            {
                _Context.Autors.Remove(autor);
                await _Context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
