using System;
using System.Collections.Generic;

namespace BibliotecaMetropolis.Models.DB;

public partial class PalabraClave
{
    public int IdPalabraClave { get; set; }

    public string Palabra { get; set; } = null!;

    public virtual ICollection<Recurso> IdRecs { get; set; } = new List<Recurso>();
}
