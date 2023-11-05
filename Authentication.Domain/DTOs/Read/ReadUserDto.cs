namespace Authentication.Domain.DTOs.Read;

public record ReadUserDto : ReadSimpleUserDto
{
	public string Email { get; init; }
	public bool IsActive { get; set; }
}