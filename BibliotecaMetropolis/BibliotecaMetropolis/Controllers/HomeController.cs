using BibliotecaMetropolis.Models;
using BibliotecaMetropolis.Models.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

/*Integrantes:
 Castellón Hernández, Emily Alessandra
 López Avelar, Vladimir Alexander
 Martínez Nolasco, Julio César
 Peñate Valle, William Eliseo
 Rivera Linares, Julio David
 */


namespace BibliotecaMetropolis.Controllers
{
    public class HomeController : Controller
    {
        private readonly BibliotecaMetropolisContext _context;

        public HomeController(BibliotecaMetropolisContext context)
        {
            _context = context;
        }

        // GET: /Home/Index?q=...
        public async Task<IActionResult> Index(string q)
        {
            // Validar que haya sesión y token
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            // Verificar expiración del JWT
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            if (jwt.ValidTo < DateTime.UtcNow)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            // Crear el ViewModel principal del dashboard
            var vm = new DashboardViewModel
            {
                UsuarioNombre = HttpContext.Session.GetString("NombreUsuario"),
                Rol = HttpContext.Session.GetString("Rol"),
                SearchTerm = q
            };

            // Contadores principales
            vm.RecursosCount = await _context.Recursos.CountAsync();
            vm.EditorialesCount = await _context.Editorials.CountAsync();

            // Query base
            var query = _context.Recursos
                .Include(r => r.IdEditNavigation)
                .AsQueryable();

            // Filtro de búsqueda
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(r => r.Titulo.Contains(term) ||
                    (r.PalabrasBusqueda != null && r.PalabrasBusqueda.Contains(term)) ||
                    (r.Descripcion != null && r.Descripcion.Contains(term)) ||
                    (r.IdEditNavigation != null && r.IdEditNavigation.Nombre.Contains(term))
                );
            }

            // Traer recursos limitados
            var recursos = await query
                .OrderByDescending(r => r.AnioPublicacion ?? 0)
                .Take(24)
                .ToListAsync();

            // Obtener autores principales
            var recIds = recursos.Select(r => r.IdRec).ToList();
            var autoresPrincipales = await _context.AutoresRecursos
                .Include(ar => ar.IdAutorNavigation)
                .Where(ar => recIds.Contains(ar.IdRec) && ar.EsPrincipal)
                .ToListAsync();

            // Convertir a viewmodel
            vm.Recursos = recursos.Select(r =>
            {
                var autorEntry = autoresPrincipales.FirstOrDefault(a => a.IdRec == r.IdRec);
                var autorNombre = autorEntry?.IdAutorNavigation != null
                    ? $"{autorEntry.IdAutorNavigation.Nombres} {autorEntry.IdAutorNavigation.Apellidos}".Trim()
                    : "Desconocido";

                return new RecursoCardViewModel
                {
                    Id = r.IdRec,
                    Titulo = r.Titulo,
                    ImagenRuta = string.IsNullOrWhiteSpace(r.ImagenRuta) ? null : r.ImagenRuta,
                    Autor = autorNombre,
                    Editorial = r.IdEditNavigation?.Nombre
                };
            }).ToList();

            return View(vm);
        }
    }
}
