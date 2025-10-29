using System.ComponentModel.DataAnnotations;

namespace BibliotecaMetropolis.Models
{
    public class UsuarioEditViewModel
    {
        public int IdUsuario { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string? Contrasena { get; set; }

        [DataType(DataType.Password)]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar contraseña")]
        public string? ConfirmContrasena { get; set; }

        [StringLength(200)]
        [Display(Name = "Nombre completo")]
        public string? NombreCompleto { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Rol")]
        [Required(ErrorMessage = "Debes seleccionar un rol.")]
        public int IdRol { get; set; }
    }
}
