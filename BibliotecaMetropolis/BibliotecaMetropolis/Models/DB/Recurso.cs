using System;
using System.Collections.Generic;

namespace BibliotecaMetropolis.Models.DB;

public partial class Recurso
{
    public int IdRec { get; set; }

    public int? IdPais { get; set; }

    public int IdTipoR { get; set; }

    public int? IdEdit { get; set; }

    public int? IdInstitucion { get; set; }

    public string Titulo { get; set; } = null!;

    public int? AnioPublicacion { get; set; }

    public string? Edicion { get; set; }

    public string? PalabrasBusqueda { get; set; }

    public string? Descripcion { get; set; }

    public decimal? Precio { get; set; }

    public int? Cantidad { get; set; }

    public virtual ICollection<AutoresRecurso> AutoresRecursos { get; set; } = new List<AutoresRecurso>();

    public virtual Editorial? IdEditNavigation { get; set; }

    public virtual Pais? IdPaisNavigation { get; set; }

    public virtual TipoRecurso IdTipoRNavigation { get; set; } = null!;

    public virtual ICollection<PalabraClave> IdPalabraClaves { get; set; } = new List<PalabraClave>();
}
