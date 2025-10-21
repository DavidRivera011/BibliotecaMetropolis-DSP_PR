using System;
using System.Collections.Generic;

namespace BibliotecaMetropolis.Models.DB;

public partial class Autor
{
    public int IdAutor { get; set; }

    public string Nombres { get; set; } = null!;

    public string? Apellidos { get; set; }

    public virtual ICollection<AutoresRecurso> AutoresRecursos { get; set; } = new List<AutoresRecurso>();
}
