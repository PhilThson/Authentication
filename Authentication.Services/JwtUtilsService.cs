using Authentication.Core.Settings;
using Authentication.Domain.Interfaces.Repositories;
using Authentication.Domain.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Authentication.Core.Constants;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Authentication.Domain.Interfaces.Services;

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
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Name),
                new Claim(AuthConstants.UserIdClaim, user.Id.ToString())
            };

            if (!int.TryParse(_jwtSettings.ExpirationTimeMin, out int expiration))
                throw new Exception("Error parsing token expiration time");

            var tokenDescriptior = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.Add(TimeSpan.FromMinutes(expiration)),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptior);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var tokenIsUnique = !_unitOfWork.User.Exists(u => u.RefreshToken == token);

            if (!tokenIsUnique)
                return GenerateRefreshToken();

            return token;
        }
    }
}

