using BibliotecaMetropolis.Models;
using BibliotecaMetropolis.Models.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class RecursosController : Controller
    {
        private readonly BibliotecaMetropolisContext _context;

        public RecursosController(BibliotecaMetropolisContext context)
        {
            _context = context;
        }

        // GET: /Recursos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var recurso = await _context.Recursos
                .Include(r => r.IdEditNavigation)
                .Include(r => r.IdPaisNavigation)
                .Include(r => r.IdTipoRNavigation)
                .Include(r => r.IdPalabraClaves) // cargar tags también
                .FirstOrDefaultAsync(r => r.IdRec == id);

            if (recurso == null) return NotFound();

            // autores (todos, marcando principal)
            var autores = await _context.AutoresRecursos
                .Include(ar => ar.IdAutorNavigation)
                .Where(ar => ar.IdRec == id)
                .OrderByDescending(ar => ar.EsPrincipal)
                .ToListAsync();

            var autoresStr = autores.Any()
                ? string.Join(", ", autores.Select(a => $"{a.IdAutorNavigation?.Nombres} {a.IdAutorNavigation?.Apellidos}".Trim()))
                : "Desconocido";

            // ViewModel simple para la vista
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

        // GET: /Recursos/Create
        public async Task<IActionResult> Create()
        {
            var vm = new RecursoEditViewModel
            {
                // valores por defecto si quieres
            };

            ViewData["Tipos"] = await _context.TipoRecursos.OrderBy(t => t.Nombre).ToListAsync();
            ViewData["Paises"] = await _context.Pais.OrderBy(p => p.Nombre).ToListAsync();
            ViewData["Editoriales"] = await _context.Editorials.OrderBy(e => e.Nombre).ToListAsync();
            ViewData["Autores"] = await _context.Autors.OrderBy(a => a.Nombres).ThenBy(a => a.Apellidos).ToListAsync();

            return View(vm);
        }

        // POST: /Recursos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecursoEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Tipos"] = await _context.TipoRecursos.OrderBy(t => t.Nombre).ToListAsync();
                ViewData["Paises"] = await _context.Pais.OrderBy(p => p.Nombre).ToListAsync();
                ViewData["Editoriales"] = await _context.Editorials.OrderBy(e => e.Nombre).ToListAsync();
                ViewData["Autores"] = await _context.Autors.OrderBy(a => a.Nombres).ThenBy(a => a.Apellidos).ToListAsync();
                return View(model);
            }

            // Crear entidad Recurso
            var recurso = new Recurso
            {
                ImagenRuta = string.IsNullOrWhiteSpace(model.ImagenRuta) ? null : model.ImagenRuta.Trim(),
                Titulo = model.Titulo?.Trim() ?? string.Empty,
                Descripcion = string.IsNullOrWhiteSpace(model.Descripcion) ? null : model.Descripcion.Trim(),
                AnioPublicacion = model.AnioPublicacion,
                Edicion = string.IsNullOrWhiteSpace(model.Edicion) ? null : model.Edicion.Trim(),
                Cantidad = model.Cantidad,
                IdTipoR = model.IdTipoR,
                IdPais = model.IdPais,
                Precio = model.Precio,
                IdEdit = model.IdEdit
            };

            // Añadir recurso al contexto (todavía no SaveChanges)
            _context.Recursos.Add(recurso);

            // ---------- Manejo de Tags (PalabraClave) ----------
            var tagsRaw = model.TagsCsv ?? string.Empty;
            var tags = tagsRaw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(t => t.Trim())
                              .Where(t => !string.IsNullOrWhiteSpace(t))
                              .Select(t => t.Length > 100 ? t.Substring(0, 100) : t)
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .Take(8)
                              .ToList();

            // Actualizar PalabrasBusqueda opcionalmente
            recurso.PalabrasBusqueda = tags.Any() ? string.Join(", ", tags) : null;

            var normalized = tags.Select(t => t.ToLowerInvariant()).ToList();
            var existingPalabras = new List<PalabraClave>();
            if (normalized.Any())
            {
                existingPalabras = await _context.PalabraClaves
                    .Where(p => normalized.Contains(p.Palabra.ToLower()))
                    .ToListAsync();
            }

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
            // ----------------------------------------------------

            // ---------- Manejo de Autores seleccionados ----------
            // El view debe mandar SelectedAuthorIds como lista (name="SelectedAuthorIds")
            if (model.SelectedAuthorIds != null && model.SelectedAuthorIds.Any())
            {
                foreach (var idAutor in model.SelectedAuthorIds.Distinct())
                {
                    var ar = new AutoresRecurso { IdRec = recurso.IdRec, IdAutor = idAutor, EsPrincipal = false };
                    // Nota: recurso.IdRec no estará asignado hasta SaveChanges, pero al agregar al contexto funciona si en la entidad la relación es agregada correctamente.
                    // Añadimos la relación en la tabla intermedia directamente al contexto:
                    _context.AutoresRecursos.Add(ar);
                }
            }
            // ----------------------------------------------------------------

            // Guardar todo
            await _context.SaveChangesAsync();

            // Redirigir a detalles del recurso creado
            return RedirectToAction("Details", new { id = recurso.IdRec });
        }


        // GET: Recurso/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Cargar recurso junto con las palabras clave relacionadas para mostrar los chips en la vista
            var recurso = await _context.Recursos
                .Include(r => r.IdPalabraClaves)
                .FirstOrDefaultAsync(r => r.IdRec == id);

            if (recurso == null) return NotFound();

            var vm = new RecursoEditViewModel
            {
                IdRec = recurso.IdRec,
                ImagenRuta = recurso.ImagenRuta,
                Titulo = recurso.Titulo,
                Descripcion = recurso.Descripcion,
                AnioPublicacion = recurso.AnioPublicacion,
                Edicion = recurso.Edicion,
                Cantidad = recurso.Cantidad,
                IdTipoR = recurso.IdTipoR,
                IdPais = recurso.IdPais,
                Precio = recurso.Precio,
                PalabrasBusqueda = recurso.PalabrasBusqueda,
                IdEdit = recurso.IdEdit,
                // convertir las PalabraClave relacionadas a lista de strings para renderizar chips
                Tags = recurso.IdPalabraClaves?.Select(pk => pk.Palabra).ToList() ?? new List<string>()
            };

            ViewData["Tipos"] = await _context.TipoRecursos.OrderBy(t => t.Nombre).ToListAsync();
            ViewData["Paises"] = await _context.Pais.OrderBy(p => p.Nombre).ToListAsync();
            ViewData["Editoriales"] = await _context.Editorials.OrderBy(e => e.Nombre).ToListAsync();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RecursoEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Tipos"] = await _context.TipoRecursos.OrderBy(t => t.Nombre).ToListAsync();
                ViewData["Paises"] = await _context.Pais.OrderBy(p => p.Nombre).ToListAsync();
                ViewData["Editoriales"] = await _context.Editorials.OrderBy(e => e.Nombre).ToListAsync();
                return View(model);
            }

            // Cargar recurso incluyendo la colección de PalabraClave para poder modificarla
            var recurso = await _context.Recursos
                .Include(r => r.IdPalabraClaves)
                .FirstOrDefaultAsync(r => r.IdRec == model.IdRec);

            if (recurso == null) return NotFound();

            // Actualizar campos
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

            // ---------- NUEVO: manejar TagsCsv y relación PalabraClave ------------
            var tagsRaw = model.TagsCsv ?? string.Empty;
            var tags = tagsRaw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(t => t.Trim())
                              .Where(t => !string.IsNullOrWhiteSpace(t))
                              .Select(t => t.Length > 100 ? t.Substring(0, 100) : t)
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .Take(8)
                              .ToList();

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
            // ---------------------------------------------------------------------

            // ---------- NUEVO: manejar autores seleccionados ----------
            var selected = model.SelectedAuthorIds ?? new List<int>();

            // limpiar relaciones actuales
            var currentARs = await _context.AutoresRecursos
                .Where(ar => ar.IdRec == recurso.IdRec)
                .ToListAsync();

            _context.AutoresRecursos.RemoveRange(currentARs);

            // añadir nuevas relaciones
            foreach (var idAutor in selected.Distinct())
            {
                _context.AutoresRecursos.Add(new AutoresRecurso
                {
                    IdRec = recurso.IdRec,
                    IdAutor = idAutor,
                    EsPrincipal = false
                });
            }
            // -----------------------------------------------------------

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


        // POST: /Recursos/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var recurso = await _context.Recursos.FindAsync(id);
            if (recurso == null) return NotFound();

            //Sólo elimina si la cantidad == 0 o tiene valor null
            if ((recurso.Cantidad ?? 0) > 0)
            {
                TempData["Error"] = "El recurso no puede eliminarse porque su cantidad es mayor a 0.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.Recursos.Remove(recurso);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Recurso eliminado correctamente.";
            return RedirectToAction("Index", "Home");
        }

        private bool RecursoExists(int id)
            => _context.Recursos.Any(e => e.IdRec == id);
    }
}
