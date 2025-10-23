using System;
using System.Collections.Generic;

namespace BibliotecaMetropolis.Models.DB;

public partial class AutoresRecurso
{
    public int IdRec { get; set; }

    public int IdAutor { get; set; }

    public bool EsPrincipal { get; set; }

    public virtual Autor IdAutorNavigation { get; set; } = null!;

    public virtual Recurso IdRecNavigation { get; set; } = null!;
}
