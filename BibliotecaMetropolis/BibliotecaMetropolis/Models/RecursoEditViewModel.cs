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

        /// <summary>
        /// Editorial seleccionada (nullable).
        /// </summary>
        public int? IdEdit { get; set; }

        /// <summary>
        /// CSV enviado desde la vista (campo oculto). Cada tag separado por coma.
        /// Validado con IValidatableObject además de este atributo.
        /// </summary>
        [StringLength(800, ErrorMessage = "TagsCsv demasiado largo.")]
        public string? TagsCsv { get; set; }

        /// <summary>
        /// para mostrar las tags actuales en la UI (GET)
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Ids de autores seleccionados en el formulario (multi-select).
        /// </summary>
        public List<int> SelectedAuthorIds { get; set; } = new List<int>();

        // ----- Configuración de límites -----
        private const int MaxTags = 8;
        private const int MaxTagLength = 100;

        /// <summary>
        /// Helper que parsea TagsCsv (o Tags si lo prefieres) en una lista limpia y normalizada.
        /// - Trim
        /// - elimina entradas vacías
        /// - truncates cada tag a MaxTagLength
        /// - distinct case-insensitive
        /// - toma hasta MaxTags
        /// </summary>
        public List<string> ParsedTags()
        {
            IEnumerable<string> source;

            if (!string.IsNullOrWhiteSpace(TagsCsv))
            {
                source = TagsCsv!
                    .Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim());
            }
            else if (Tags != null && Tags.Any())
            {
                source = Tags.Select(t => (t ?? string.Empty).Trim());
            }
            else
            {
                source = Enumerable.Empty<string>();
            }

            var list = source
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Length > MaxTagLength ? t.Substring(0, MaxTagLength) : t)
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .Take(MaxTags)
                .ToList();

            return list;
        }

        // Validaciones personalizadas:
        // - máximo 8 tags
        // - cada tag máximo 100 caracteres
        // - SelectedAuthorIds no contiene ids negativos
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            var tagsList = ParsedTags();

            if (tagsList.Count > MaxTags)
            {
                results.Add(new ValidationResult($"Máximo {MaxTags} etiquetas.", new[] { nameof(TagsCsv) }));
            }

            foreach (var t in tagsList)
            {
                if (t.Length > MaxTagLength)
                {
                    results.Add(new ValidationResult($"La etiqueta '{t}' no puede exceder {MaxTagLength} caracteres.", new[] { nameof(TagsCsv) }));
                }
            }

            // Validación sencilla sobre SelectedAuthorIds (no negativos).
            if (SelectedAuthorIds != null && SelectedAuthorIds.Any(id => id < 0))
            {
                results.Add(new ValidationResult("Lista de autores contiene un id no válido.", new[] { nameof(SelectedAuthorIds) }));
            }

            // -> Puedes añadir más validaciones aquí (ej: máximo X autores, reglas sobre título, etc.)

            return results;
        }
    }
}
