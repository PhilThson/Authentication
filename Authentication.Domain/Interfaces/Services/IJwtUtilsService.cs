using Authentication.Domain.Models;

namespace Authentication.Domain.Interfaces.Services
{
    public interface IJwtUtilsService
    {
        string GenerateRefreshToken();
        string GenerateToken(User user);
    }
}