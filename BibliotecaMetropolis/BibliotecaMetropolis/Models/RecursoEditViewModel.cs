using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BibliotecaMetropolis.Models
{
    public class RecursoEditViewModel : IValidatableObject
    {
        public int IdRec { get; set; }

        [Required]
        [StringLength(250)]
        public string Titulo { get; set; } = string.Empty;

        public string? ImagenRuta { get; set; }
        public int? AnioPublicacion { get; set; }
        public string? Edicion { get; set; }

        public string? PalabrasBusqueda { get; set; }

        public string? Descripcion { get; set; }
        public decimal? Precio { get; set; }
        public int? Cantidad { get; set; }
        public int IdTipoR { get; set; }
        public int? IdPais { get; set; }

        public int? IdEdit { get; set; }

        [StringLength(800, ErrorMessage = "TagsCsv demasiado largo.")]
        public string? TagsCsv { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public List<int> SelectedAuthorIds { get; set; } = new List<int>();


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();


            var normalizedAuthors = (SelectedAuthorIds ?? new List<int>())
                .Where(id => id > 0)
                .ToList();

            // Límite máximo 3 autores
            if (normalizedAuthors.Count > 3)
            {
                results.Add(new ValidationResult("Sólo se permiten hasta 3 autores (1 principal y 2 opcionales).", new[] { nameof(SelectedAuthorIds) }));
            }

            // Debe existir al menos un autor (el principal)
            if (!normalizedAuthors.Any())
            {
                results.Add(new ValidationResult("Debes seleccionar al menos un autor (principal).", new[] { nameof(SelectedAuthorIds) }));
            }

            // Duplicados
            if (normalizedAuthors.Distinct().Count() != normalizedAuthors.Count)
            {
                results.Add(new ValidationResult("No puedes seleccionar el mismo autor en varias casillas.", new[] { nameof(SelectedAuthorIds) }));
            }

            SelectedAuthorIds = normalizedAuthors;

            return results;
        }
    }
}