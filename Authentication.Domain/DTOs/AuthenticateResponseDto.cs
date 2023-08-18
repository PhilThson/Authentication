namespace Authentication.Domain.DTOs;

public class AuthenticateResponseDto
{
    public string? JwtToken { get; set; }
    public string? RefreshToken { get; set; }
}

