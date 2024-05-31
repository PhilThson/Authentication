using Authentication.Core.Settings;
using Authentication.Domain.Interfaces.Repositories;
using Authentication.Domain.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Authentication.Core.Constants;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Authentication.Domain.Interfaces.Services;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Authentication.Services
{
    public class JwtUtilsService : IJwtUtilsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;
        private readonly string _jwtKey;

        public JwtUtilsService(IUnitOfWork unitOfWork,
            IOptions<JwtSettings> jwtSettings,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettings.Value;
            _jwtKey = configuration.GetSection(AuthConstants.JwtKey).Value;
        }

        public string GenerateToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_jwtKey);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, user.Name),
                new(AuthConstants.UserIdClaim, user.Id.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                IssuedAt = null,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationTimeMin),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            JsonWebTokenHandler tokenHandler = new()
            {
                SetDefaultTimesOnTokenCreation = false
            };
            return tokenHandler.CreateToken(tokenDescriptor);
        }

        public string GenerateRefreshToken()
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var tokenIsUnique = !_unitOfWork.User.Exists(u => u.RefreshToken == token);

            return !tokenIsUnique ? GenerateRefreshToken() : token;
        }
    }
}

