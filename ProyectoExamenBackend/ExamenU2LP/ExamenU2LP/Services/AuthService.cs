using ExamenU2LP.Databases.LogDatabase;
using ExamenU2LP.Databases.LogDatabase.Entities;
using ExamenU2LP.Databases.TransactionalDatabase;
using ExamenU2LP.Databases.TransactionalDatabase.Entities;
using ExamenU2LP.Dtos.Auth;
using ExamenU2LP.Dtos.Common;
using ExamenU2LP.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ExamenU2LP.Services;

//revisar el tiempo si es muy poco de validacion del token
public class AuthService : IAuthService
{
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly TransactionalContext _transactionalContext;
    private readonly LogContext _logContext;

    public AuthService(
            SignInManager<UserEntity> signInManager,
            UserManager<UserEntity> userManager,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            TransactionalContext transactionalContext,
            LogContext logContext
        )
    {
        this._signInManager = signInManager;
        this._userManager = userManager;
        this._configuration = configuration;
        this._logger = logger;
        this._transactionalContext = transactionalContext;
        this._logContext = logContext;
    }

    //login
    public async Task<ResponseDto<LoginResponseDto>> LoginAsync (LoginDto dto)
    {
        var result = await _signInManager
            .PasswordSignInAsync(dto.Email,
            dto.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        var logEntity = new LogEntity
        {
            Id = Guid.NewGuid(),
            Date = DateTime.Now,
            Action = result.Succeeded ? "Sesión iniciada correctamente" : "Error al iniciar sesión, intentelo más tarde",
            UserId = result.Succeeded ? (await _userManager.FindByEmailAsync(dto.Email)).Id : null
        };

        await _logContext.Logs.AddAsync(logEntity);
        await _logContext.SaveChangesAsync();

        if (result.Succeeded)
        {
            var userEntity = await _userManager.FindByEmailAsync(dto.Email);

            List<Claim> authClaims = await GetClaims(userEntity);

            var jwtToken = GetToken(authClaims);

            var refreshToken = GenerateRefreshTokenString();

            //guardar refreshtoken en la base de datos
            userEntity.RefreshToken = refreshToken;
            userEntity.RefreshTokenExpire = DateTime.Now
                .AddHours(int.Parse(_configuration["JWT:RefreshTokenExpire"] ?? "10"));     //esto se dejó en horas
            //userEntity.RefreshTokenExpire = DateTime.Now
                //.AddMinutes(int.Parse(_configuration["JWT:RefreshTokenExpire"] ?? "30"));     //esto se dejó en minutos

            _transactionalContext.Entry(userEntity);
            await _transactionalContext.SaveChangesAsync();

            //aqui habria que crear un log en que la accion sea, Inicio de Sesion satisfactorio
            //y guardarlo en la base de datos de logs

            return new ResponseDto<LoginResponseDto>
            {
                StatusCode = 200,
                Status = true,
                Message = "Inicio de sesión satisfactorio",
                Data = new LoginResponseDto
                {
                    FullName = $"{userEntity.FirstName} {userEntity.LastName}",
                    Email = userEntity.Email,       //o dto.Email
                    Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    TokenExpiration = jwtToken.ValidTo,
                    RefreshToken = refreshToken
                }
            };

        }

        //aqui habria que crear un log en que la accion sea, Inicio de Sesion Falló
        //y guardarlo en la base de datos de logs

        return new ResponseDto<LoginResponseDto>
        {
            Status = false,
            StatusCode = 401,
            Message = "Fallo el inicio de sesión"
        };
    }

    //posiblemente incorporar un metodo de cerrar sesion

    //register      (este no será necesario)
    //refreshtoken      (este no será necesario)

    //metodos adicionales
    private async Task<List<Claim>> GetClaims(UserEntity userEntity)
    {
        var authClaims = new List<Claim>
            {
                //esto no esta hasheado se manda como un base64
                new Claim(ClaimTypes.Email, userEntity.Email),  //email
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  //guid unico
                new Claim("UserId", userEntity.Id)  //id JwtRegisteredClaimNames
            };

        //ver que roles tiene asignado el usuario
        var userRoles = await _userManager.GetRolesAsync(userEntity);

        foreach (var role in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        return authClaims;
    }

    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        //SymmetricSecurityKey convierte a bites algo
        var authSigninKey = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(_configuration["JWT:Secret"]));

        return new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(int.Parse(_configuration["JWT:Expires"] ?? "5")),          //esto se dejó en Horas
                //expires: DateTime.Now.AddMinutes(int.Parse(_configuration["JWT:Expires"] ?? "15")),          //esto se dejó en Minutos
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigninKey,
                    SecurityAlgorithms.HmacSha256)
            );

    }

    private string GenerateRefreshTokenString()
    {
        var randomNumber = new byte[64];

        using (var numberGenerator = RandomNumberGenerator.Create())
        {
            numberGenerator.GetBytes(randomNumber);
        }

        return Convert.ToBase64String(randomNumber);
    }

}
