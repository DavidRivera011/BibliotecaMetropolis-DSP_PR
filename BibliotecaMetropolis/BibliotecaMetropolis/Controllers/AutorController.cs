using BibliotecaMetropolis.Models.DB;
using Microsoft.AspNetCore.Mvc;
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
    [Route("api/[controller]")]
    [ApiController]
    public class AutorController : ControllerBase
    {
        private readonly BibliotecaMetropolisContext _context;

        public AutorController(BibliotecaMetropolisContext context)
        {
            _context = context;
        }

        public class AutorCreateDto
        {
            public string Nombres { get; set; } = string.Empty;
            public string? Apellidos { get; set; }
        }

        // POST: api/Autor
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AutorCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombres))
                return BadRequest(new { error = "El nombre es obligatorio." });

            var autor = new Autor
            {
                Nombres = dto.Nombres.Trim(),
                Apellidos = string.IsNullOrWhiteSpace(dto.Apellidos) ? null : dto.Apellidos.Trim()
            };

            _context.Autors.Add(autor);
            await _context.SaveChangesAsync();

            return Ok(new { id = autor.IdAutor, nombre = $"{autor.Nombres} {autor.Apellidos ?? ""}".Trim() });
        }
    }
}
