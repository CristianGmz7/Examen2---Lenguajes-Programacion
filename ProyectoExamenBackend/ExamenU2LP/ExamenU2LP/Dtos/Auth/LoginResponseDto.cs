namespace ExamenU2LP.Dtos.Auth;

public class LoginResponseDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    public DateTime TokenExpiration { get; set; }
    public string RefreshToken { get; set; }
}
