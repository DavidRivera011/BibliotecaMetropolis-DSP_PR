using System;
using System.Collections.Generic;

namespace BibliotecaMetropolis.Models.DB;

public partial class Pais
{
    public int IdPais { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Recurso> Recursos { get; set; } = new List<Recurso>();
}
