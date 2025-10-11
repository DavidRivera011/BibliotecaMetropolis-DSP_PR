using System;
using System.Collections.Generic;

namespace BibliotecaMetropolis.Models.DB;

public partial class Institucion
{
    public int IdInstitucion { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
}
