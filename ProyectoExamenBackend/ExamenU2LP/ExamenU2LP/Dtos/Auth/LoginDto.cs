using System.ComponentModel.DataAnnotations;

namespace ExamenU2LP.Dtos.Auth;

public class LoginDto
{
    [Display(Name = "Correo Electronico")]
    [Required(ErrorMessage = "El {0} es obligatorio")]
    public string Email { get; set; }

    [Display(Name = "Contraseña")]
    [Required(ErrorMessage = "La {0} es obligatoria")]
    public string Password { get; set; }
}
