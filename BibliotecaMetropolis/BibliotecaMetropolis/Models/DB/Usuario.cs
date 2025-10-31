using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaMetropolis.Models.DB;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    public string NombreUsuario { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = null!;

    public string? NombreCompleto { get; set; }

    public bool Activo { get; set; }

    public int IdRol { get; set; }

    public virtual Rol? IdRolNavigation { get; set; }
}
