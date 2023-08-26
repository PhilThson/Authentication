using Authentication.Domain.DTOs.Common;
using Authentication.Domain.DTOs.Create;
using Authentication.Domain.DTOs.Read;

namespace Authentication.Domain.Interfaces.Services
{
    public interface IUserService
    {
        Task<AuthenticateResponseDto> Authenticate(AuthenticateRequestDto model);
        Task<AuthenticateResponseDto> RefreshToken(string refreshToken);
        Task<IEnumerable<ReadSimpleUserDto>> GetAll(string ids);
        Task<ReadUserDto> GetById(int id);
        Task<ReadUserDto> Register(CreateUserDto createUserDto);
    }
}