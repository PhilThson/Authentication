using Authentication.Core.Exceptions;
using Authentication.Core.Settings;
using Authentication.Domain.DTOs;
using Authentication.Domain.Interfaces.Repositories;
using Authentication.Domain.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace Authentication.Services;

public class UserService : IUserService
{
    private readonly IJwtUtilsService _jwtUtils;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;


    public UserService(IJwtUtilsService jwtUtils,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> options)
    {
        _jwtUtils = jwtUtils;
        _unitOfWork = unitOfWork;
        _jwtSettings = options.Value;
    }

    //public async Task<IEnumerable<T>> GetAll<T>() =>
    //    _mapper.Map<IEnumerable<T>>(await _unitOfWork.User.GetAllAsync());

    public async Task<AuthenticateResponseDto> Authenticate(AuthenticateRequestDto model)
    {
        var user = await _unitOfWork.User.GetFirstAsync(u => u.Email == model.Email) ??
            throw new NotFoundException("User not exists");

        if (!user.IsActive)
            throw new UnauthorizedAccessException();

        var expirationDays = GetRefreshTokenExpirationTime();

        var jwtToken = _jwtUtils.GenerateToken(user);
        user.RefreshToken = _jwtUtils.GenerateRefreshToken();
        user.RefreshTokenExpiration = DateTime.Now.AddDays(expirationDays);

        _unitOfWork.Save();

        return new AuthenticateResponseDto
        {
            JwtToken = jwtToken,
            RefreshToken = user.RefreshToken
        };
    }

    public async Task<AuthenticateResponseDto> RefreshToken(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new DataValidationException("No refresh token provided");

        var user = await _unitOfWork.User.GetFirstAsync(u => u.RefreshToken == refreshToken) ??
            throw new DataValidationException("Invalid token provided");

        if (user.RefreshTokenIsRevoked)
            throw new AuthenticationException("Refresh token is revoked");

        if (user.RefreshTokenExpiration!.Value < DateTime.Now)
            throw new AuthenticationException("Refresh token expired");

        var expirationDays = GetRefreshTokenExpirationTime();

        var newRefreshToken = _jwtUtils.GenerateRefreshToken();
        var newJwtToken = _jwtUtils.GenerateToken(user);

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiration = DateTime.Now.AddDays(expirationDays);

        _unitOfWork.Save();

        return new AuthenticateResponseDto
        {
            JwtToken = newJwtToken,
            RefreshToken = newRefreshToken
        };
    }

    private int GetRefreshTokenExpirationTime()
    {
        if (!int.TryParse(_jwtSettings.RefreshTokenExpirationTimeDays, out int expirationDays))
            throw new Exception("Error parsing refresh token expiration time");

        return expirationDays;
    }
}