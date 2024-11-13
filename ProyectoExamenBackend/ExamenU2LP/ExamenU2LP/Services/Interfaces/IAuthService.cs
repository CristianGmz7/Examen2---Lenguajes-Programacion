using ExamenU2LP.Dtos.Auth;
using ExamenU2LP.Dtos.Common;

namespace ExamenU2LP.Services.Interfaces;

public interface IAuthService
{
    Task<ResponseDto<LoginResponseDto>> LoginAsync(LoginDto dto);
}
