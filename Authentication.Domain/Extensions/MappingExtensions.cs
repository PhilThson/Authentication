using Authentication.Domain.DTOs.Read;
using Authentication.Domain.Models;

namespace Authentication.Domain.Extensions
{
    public static class MappingExtensions
	{
		public static ReadUserDto MapToReadDto(this User user)
		{
			return new ReadUserDto
			{
				Id = user.Id,
				Name = user.Name,
				Email = user.Email
			};
		}
	}
}

