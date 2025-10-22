using System.ComponentModel.DataAnnotations;

namespace BibliotecaMetropolis.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder los 50 caracteres.")]
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(255, ErrorMessage = "La contraseña no puede exceder los 255 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = null!;

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre completo no puede exceder los 100 caracteres.")]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = null!;

        [Display(Name = "Cuenta activa")]
        public bool Activo { get; set; } = true;

        public int IdRol { get; set; } = 1;
    }
}
