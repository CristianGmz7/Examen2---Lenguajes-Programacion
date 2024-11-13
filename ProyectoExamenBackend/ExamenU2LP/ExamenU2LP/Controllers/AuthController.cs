using ExamenU2LP.Dtos.Auth;
using ExamenU2LP.Dtos.Common;
using ExamenU2LP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExamenU2LP.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(
            IAuthService authService
        )
    {
        this._authService = authService;
    }

    //login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ResponseDto<LoginResponseDto>>> Login (LoginDto dto)
    {
        var response = await _authService.LoginAsync(dto);
        return StatusCode(response.StatusCode, response);
    }

    //register (no será necesario)

    //refreshtoken (no será necesario)
}
