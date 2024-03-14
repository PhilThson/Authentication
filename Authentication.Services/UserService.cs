using Authentication.Core.Exceptions;
using Authentication.Core.Security;
using Authentication.Core.Settings;
using Authentication.Domain.DTOs.Common;
using Authentication.Domain.DTOs.Create;
using Authentication.Domain.DTOs.Read;
using Authentication.Domain.Extensions;
using Authentication.Domain.Interfaces.Repositories;
using Authentication.Domain.Interfaces.Services;
using Authentication.Domain.Models;
using Microsoft.Extensions.Options;

namespace Authentication.Services;

public class UserService : IUserService
{
    #region Private fields
    private readonly IJwtUtilsService _jwtUtils;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;
    #endregion

    #region Constructor
    public UserService(IJwtUtilsService jwtUtils,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> options)
    {
        _jwtUtils = jwtUtils;
        _unitOfWork = unitOfWork;
        _jwtSettings = options.Value;
    }
    #endregion

    #region Methods

    #region Authenticate

    public async Task<AuthenticateResponseDto> Authenticate(AuthenticateRequestDto authRequest)
    {
        var user = await _unitOfWork.User.GetFirstAsync(u => u.Email == authRequest.Email) ??
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!PasswordHasher.Verify(authRequest.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User blocked. Please contact administrator.");
        
        var jwtToken = _jwtUtils.GenerateToken(user);
        user.RefreshToken = _jwtUtils.GenerateRefreshToken();
        user.RefreshTokenExpiration = DateTime.Now.AddDays(_jwtSettings.RefreshTokenExpirationTimeDays);

        await _unitOfWork.SaveAsync();

        return new AuthenticateResponseDto
        {
            JwtToken = jwtToken,
            RefreshToken = user.RefreshToken,
            RefreshTokenExpirationTimeDays = _jwtSettings.RefreshTokenExpirationTimeDays
        };
    }

    #endregion

    #region Refresh token

    public async Task<AuthenticateResponseDto> RefreshToken(string? refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new DataValidationException("No refresh token provided");

        var user = await _unitOfWork.User.GetFirstAsync(u => u.RefreshToken == refreshToken) ??
            throw new DataValidationException("Invalid token provided");

        if (user.RefreshTokenIsRevoked)
            throw new AuthenticationException("Refresh token is revoked");

        if (user.RefreshTokenExpiration!.Value < DateTime.Now)
            throw new AuthenticationException("Refresh token expired");
        
        var newRefreshToken = _jwtUtils.GenerateRefreshToken();
        var newJwtToken = _jwtUtils.GenerateToken(user);

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiration = DateTime.Now.AddDays(_jwtSettings.RefreshTokenExpirationTimeDays);

        await _unitOfWork.SaveAsync();

        return new AuthenticateResponseDto
        {
            JwtToken = newJwtToken,
            RefreshToken = newRefreshToken,
            RefreshTokenExpirationTimeDays = _jwtSettings.RefreshTokenExpirationTimeDays
        };
    }

    #endregion

    #region Get all users
    public async Task<IEnumerable<ReadSimpleUserDto>> GetAll(string? ids)
    {
        if (!string.IsNullOrEmpty(ids))
            return await GetByIds(ids);

        var allUsers = await _unitOfWork.User.GetAllAsync();
        return allUsers.Select(u => u.MapToSimpleDto());
    }
    #endregion

    #region Get User by Id

    public async Task<ReadUserDto> GetById(int id)
    {
        if (id == default)
            throw new DataValidationException("Invalid user identifier");

        var user = await _unitOfWork.User.GetByIdAsync(id) ??
            throw new NotFoundException("User not found");

        return user.MapToReadDto();
    }

    #endregion

    #region Register user

    public async Task<ReadUserDto> Register(CreateUserDto createUserDto)
    {
        if (_unitOfWork.User.Exists(u => u.Email == createUserDto.Email))
            throw new DataValidationException("Email address is already registered");

        if (_unitOfWork.User.Exists(u => u.Name == createUserDto.Name))
            throw new DataValidationException("Given Name is already taken");

        var user = new User
        {
            Name = createUserDto.Name,
            Email = createUserDto.Email,
            PasswordHash = PasswordHasher.Hash(createUserDto.Password)
        };

        _unitOfWork.User.Add(user);
        await _unitOfWork.SaveAsync();

        return user.MapToReadDto();
    }

    #endregion

    #region Private methods

    private async Task<IEnumerable<ReadSimpleUserDto>> GetByIds(string ids)
    {
        var userIdsString = ids.Split(",", StringSplitOptions.TrimEntries).ToList();
        var userIds = userIdsString
            .Select(s => int.TryParse(s, out int id) ? id : 0)
            .Distinct();

        var users = await _unitOfWork.User.GetByConditionAsync(u =>
            userIds.Contains(u.Id));

        return users.Select(u => u.MapToSimpleDto());
    }

    #endregion

    #endregion
}