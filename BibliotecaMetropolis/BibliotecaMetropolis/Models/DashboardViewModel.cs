using System.Collections.Generic;

namespace BibliotecaMetropolis.Models
{
    public class RecursoCardViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? ImagenRuta { get; set; }
        public string Autor { get; set; } = "Desconocido";
        public string? Editorial { get; set; }
    }

    public class DashboardViewModel
    {
        public string? UsuarioNombre { get; set; }
        public string? Rol { get; set; }

        public int RecursosCount { get; set; }
        public int EditorialesCount { get; set; }

        public string? SearchTerm { get; set; }

        public List<RecursoCardViewModel> Recursos { get; set; } = new();
    }
}
