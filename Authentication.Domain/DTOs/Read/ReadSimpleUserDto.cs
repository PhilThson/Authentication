namespace Authentication.Domain.DTOs.Read
{
    public record ReadSimpleUserDto
	{
        public int Id { get; init; }
        public string Name { get; init; }
        public string Email { get; init; }
    }
}

