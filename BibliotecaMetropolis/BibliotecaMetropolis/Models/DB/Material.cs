using System;
using System.Collections.Generic;

namespace BibliotecaMetropolis.Models.DB;

public partial class Material
{
    public int IdMaterial { get; set; }

    public string Titulo { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public int? AnioPublicacion { get; set; }

    public decimal? Precio { get; set; }

    public int? Cantidad { get; set; }

    public int IdAutor { get; set; }

    public int? IdEditorial { get; set; }

    public int? IdInstitucion { get; set; }

    public virtual Autor IdAutorNavigation { get; set; } = null!;

    public virtual Editorial? IdEditorialNavigation { get; set; }

    public virtual Institucion? IdInstitucionNavigation { get; set; }

    public virtual ICollection<PalabraClave> IdPalabraClaves { get; set; } = new List<PalabraClave>();
}
