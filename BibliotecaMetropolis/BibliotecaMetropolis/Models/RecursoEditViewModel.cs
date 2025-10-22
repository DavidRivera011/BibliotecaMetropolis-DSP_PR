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

        // Puedes eliminar PalabrasBusqueda si ya no lo usas.
        public string? PalabrasBusqueda { get; set; }

        public string? Descripcion { get; set; }
        public decimal? Precio { get; set; }
        public int? Cantidad { get; set; }
        public int IdTipoR { get; set; }
        public int? IdPais { get; set; }
        public int? IdEdit { get; set; }

        // CSV enviado desde la vista (campo oculto). Cada tag separado por coma.
        [StringLength(800, ErrorMessage = "TagsCsv demasiado largo.")]
        public string? TagsCsv { get; set; }

        // para mostrar las tags actuales en la UI (GET)
        public List<string> Tags { get; set; } = new List<string>();
        public List<int> SelectedAuthorIds { get; set; } = new List<int>();

        // Validaciones personalizadas:
        // - máximo 8 tags
        // - cada tag máximo 100 caracteres
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // Preferimos validar usando Tags (ya poblado en GET) si existe;
            // en POST el binder nos trae TagsCsv, entonces parseamos TagsCsv.
            IEnumerable<string> tagsSource = Tags ?? Enumerable.Empty<string>();

            if (string.IsNullOrWhiteSpace(TagsCsv) == false)
            {
                tagsSource = TagsCsv
                    .Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t));
            }

            var tagsList = tagsSource
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (tagsList.Count > 8)
            {
                results.Add(new ValidationResult("Máximo 8 etiquetas.", new[] { nameof(TagsCsv) }));
            }

            foreach (var t in tagsList)
            {
                if (t.Length > 100)
                {
                    results.Add(new ValidationResult($"La etiqueta '{t}' no puede exceder 100 caracteres.", new[] { nameof(TagsCsv) }));
                }
            }

            return results;
        }
    }
}
