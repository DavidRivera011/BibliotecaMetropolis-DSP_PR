using System;
using System.Collections.Generic;

namespace BibliotecaMetropolis.Models.DB;

public partial class Autor
{
    public int IdAutor { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string? Nacionalidad { get; set; }

    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
}
