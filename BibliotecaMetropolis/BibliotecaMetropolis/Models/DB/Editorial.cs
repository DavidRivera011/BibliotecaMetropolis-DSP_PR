using System;
using System.Collections.Generic;

namespace BibliotecaMetropolis.Models.DB;

public partial class Editorial
{
    public int IdEditorial { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Pais { get; set; }

    public string? Ciudad { get; set; }

    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
}
