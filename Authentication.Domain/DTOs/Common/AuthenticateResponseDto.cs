namespace Authentication.Domain.DTOs.Common;

public class AuthenticateResponseDto
{
    public string? JwtToken { get; set; }
    public string? RefreshToken { get; set; }
    public int RefreshTokenExpirationTimeDays { get; set; }
}