﻿using System.ComponentModel.DataAnnotations;

namespace ExamenU2LP.Dtos.Auth;

public class RefreshTokenDto
{
    [Required(ErrorMessage = "El Token es requerido")]
    public string Token { get; set; }

    [Required(ErrorMessage = "El RefreshToken es requerido")]
    public string RefreshToken { get; set; }
}
