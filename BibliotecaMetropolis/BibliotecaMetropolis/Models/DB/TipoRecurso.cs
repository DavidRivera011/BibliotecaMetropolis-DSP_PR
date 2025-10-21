using System;
using System.Collections.Generic;

namespace BibliotecaMetropolis.Models.DB;

public partial class TipoRecurso
{
    public int IdTipoR { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Recurso> Recursos { get; set; } = new List<Recurso>();
}
