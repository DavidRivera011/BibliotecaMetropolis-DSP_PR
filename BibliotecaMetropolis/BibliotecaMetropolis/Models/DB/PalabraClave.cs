using System;
using System.Collections.Generic;

namespace BibliotecaMetropolis.Models.DB;

public partial class PalabraClave
{
    public int IdPalabraClave { get; set; }

    public string Palabra { get; set; } = null!;

    public virtual ICollection<Material> IdMaterials { get; set; } = new List<Material>();
}
