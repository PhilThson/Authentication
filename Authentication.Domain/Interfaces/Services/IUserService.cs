using Authentication.Domain.DTOs;

namespace Authentication.Domain.Interfaces.Services
{
    public interface IUserService
    {
        Task<AuthenticateResponseDto> Authenticate(AuthenticateRequestDto model);
        Task<AuthenticateResponseDto> RefreshToken(string refreshToken);
    }
}