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
    public class EditorialController : ControllerBase
    {
        private readonly BibliotecaMetropolisContext _context;

        public EditorialController(BibliotecaMetropolisContext context)
        {
            _context = context;
        }

        public class EditorialCreateDto
        {
            public string Nombre { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
        }

        // POST: api/Editorial
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] EditorialCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { error = "El nombre de la editorial es obligatorio." });

            var editorial = new Editorial
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim()
            };

            _context.Editorials.Add(editorial);
            await _context.SaveChangesAsync();

            return Ok(new { id = editorial.IdEdit, nombre = editorial.Nombre });
        }
    }
}
