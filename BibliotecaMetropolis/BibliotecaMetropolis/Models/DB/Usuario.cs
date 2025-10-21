using System;
using System.Collections.Generic;

namespace BibliotecaMetropolis.Models.DB;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string Contrasena { get; set; } = null!; //vamos a hashear las contraseñas en la lógica de negocio porque no pueden estar en texto plano

    public string? NombreCompleto { get; set; }

    public bool Activo { get; set; }

    public int IdRol { get; set; }

    public virtual Rol IdRolNavigation { get; set; } = null!;
}
