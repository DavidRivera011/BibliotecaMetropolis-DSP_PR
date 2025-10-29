using BibliotecaMetropolis.Filtros;
using BibliotecaMetropolis.Models;
using BibliotecaMetropolis.Models.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BibliotecaMetropolis.Controllers
{
    public class RecursosController : Controller
    {
        private readonly BibliotecaMetropolisContext _context;

        public RecursosController(BibliotecaMetropolisContext context)
        {
            _context = context;
        }

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

        [RoleAuthorize("Administrador")]
        public async Task<IActionResult> Create()
        {
            var vm = new RecursoEditViewModel { };

            ViewData["Tipos"] = await _context.TipoRecursos.OrderBy(t => t.Nombre).ToListAsync();
            ViewData["Paises"] = await _context.Pais.OrderBy(p => p.Nombre).ToListAsync();
            ViewData["Editoriales"] = await _context.Editorials.OrderBy(e => e.Nombre).ToListAsync();
            ViewData["Autores"] = await _context.Autors.OrderBy(a => a.Nombres).ThenBy(a => a.Apellidos).ToListAsync();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Administrador")]
        public async Task<IActionResult> Create(RecursoEditViewModel model)
        {
            var selectedAuthorIds = model.SelectedAuthorIds ?? new List<int>();
            if ((!selectedAuthorIds.Any()) && Request.HasFormContentType && Request.Form.ContainsKey("SelectedAuthorIds"))
            {
                var vals = Request.Form["SelectedAuthorIds"].Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                foreach (var s in vals)
                {
                    if (int.TryParse(s, out var id)) selectedAuthorIds.Add(id);
                }
            }

            var orderedSelected = selectedAuthorIds.Where(id => id > 0).ToList();
            if (!orderedSelected.Any())
                ModelState.AddModelError("", "Debes seleccionar al menos un autor (principal).");
            if (orderedSelected.Distinct().Count() != orderedSelected.Count)
                ModelState.AddModelError("", "No puedes seleccionar el mismo autor en varias casillas.");

            var keywords = new List<string>();
            if (Request.HasFormContentType && Request.Form.ContainsKey("Keywords"))
            {
                keywords = Request.Form["Keywords"]
                    .Select(k => (k ?? "").Trim())
                    .Where(k => !string.IsNullOrEmpty(k))
                    .Select(k => k.Length > 100 ? k.Substring(0, 100) : k)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(8)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(model.TagsCsv))
            {
                var fromCsv = model.TagsCsv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => t.Length > 100 ? t.Substring(0, 100) : t)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(8)
                    .ToList();
                if (fromCsv.Any()) keywords = fromCsv;
            }

            model.SelectedAuthorIds = orderedSelected;
            model.Tags = keywords;

            if (!ModelState.IsValid)
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

            _context.Recursos.Add(recurso);
            await _context.SaveChangesAsync();

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

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = recurso.IdRec });
        }

        [RoleAuthorize("Administrador")]
        public async Task<IActionResult> Edit(int id)
        {
            var recurso = await _context.Recursos
                .Include(r => r.IdPalabraClaves)
                .FirstOrDefaultAsync(r => r.IdRec == id);

            if (recurso == null) return NotFound();

            var autoresRelacionados = await _context.AutoresRecursos
                .Where(ar => ar.IdRec == id)
                .OrderByDescending(ar => ar.EsPrincipal)
                .Select(ar => ar.IdAutor)
                .ToListAsync();

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
                Tags = recurso.IdPalabraClaves?.Select(pk => pk.Palabra).ToList() ?? new List<string>(),
                SelectedAuthorIds = autoresRelacionados ?? new List<int>()
            };

            ViewData["Tipos"] = await _context.TipoRecursos.OrderBy(t => t.Nombre).ToListAsync();
            ViewData["Paises"] = await _context.Pais.OrderBy(p => p.Nombre).ToListAsync();
            ViewData["Editoriales"] = await _context.Editorials.OrderBy(e => e.Nombre).ToListAsync();
            ViewData["Autores"] = await _context.Autors.OrderBy(a => a.Nombres).ThenBy(a => a.Apellidos).ToListAsync();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RecursoEditViewModel model)
        {
            var selected = model.SelectedAuthorIds ?? new List<int>();
            if ((!selected.Any()) && Request.HasFormContentType && Request.Form.ContainsKey("SelectedAuthorIds"))
            {
                var vals = Request.Form["SelectedAuthorIds"].Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                foreach (var s in vals)
                {
                    if (int.TryParse(s, out var id)) selected.Add(id);
                }
            }

            var orderedSelected = selected.Where(id => id > 0).ToList();
            if (!orderedSelected.Any())
                ModelState.AddModelError("", "Debes seleccionar al menos un autor (principal).");
            if (orderedSelected.Distinct().Count() != orderedSelected.Count)
                ModelState.AddModelError("", "No puedes seleccionar el mismo autor en varias casillas.");

            var keywords = new List<string>();
            if (Request.HasFormContentType && Request.Form.ContainsKey("Keywords"))
            {
                keywords = Request.Form["Keywords"]
                    .Select(k => (k ?? "").Trim())
                    .Where(k => !string.IsNullOrEmpty(k))
                    .Select(k => k.Length > 100 ? k.Substring(0, 100) : k)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(8)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(model.TagsCsv))
            {
                var fromCsv = model.TagsCsv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => t.Length > 100 ? t.Substring(0, 100) : t)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(8)
                    .ToList();
                if (fromCsv.Any()) keywords = fromCsv;
            }

            model.SelectedAuthorIds = orderedSelected;
            model.Tags = keywords;

            if (!ModelState.IsValid)
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

            _context.Recursos.Remove(recurso);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Recurso eliminado correctamente.";
            return RedirectToAction("Index", "Home");
        }

        private bool RecursoExists(int id)
            => _context.Recursos.Any(e => e.IdRec == id);
    }
}
