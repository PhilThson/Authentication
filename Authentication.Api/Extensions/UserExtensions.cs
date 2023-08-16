using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Authentication.Core.Constants;
using Authentication.Core.Exceptions;

namespace Authentication.Api.Extensions
{
    public static class UserExtensions
	{
        public static int GetUserId(this HttpContext httpContext)
        {
            var userIdString = httpContext.User
                ?.Claims
                ?.FirstOrDefault(c => c.Type == AuthConstants.UserIdClaim)
                ?.Value ??
                throw new UnknownUserException();

            if (!int.TryParse(userIdString, out int userId))
                throw new UnknownUserException($"Invalid User Id ({userIdString})");

            return userId;
        }

        public static string GetUserName(this ClaimsPrincipal user) =>
            user?.FindFirst(c => c.Type == JwtRegisteredClaimNames.Sub)
                ?.Value ??
                throw new UnknownUserException();

        public static string? GetToken(this HttpContext httpContext) =>
            httpContext.Request.Headers["Authorization"]
                .FirstOrDefault()
                ?.Split(" ")
                .Last();
    }
}