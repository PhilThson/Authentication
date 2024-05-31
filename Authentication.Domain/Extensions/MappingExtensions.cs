using Authentication.Domain.DTOs.Read;
using Authentication.Domain.Models;

namespace Authentication.Domain.Extensions
{
    public static class MappingExtensions
	{
		public static ReadSimpleUserDto MapToSimpleDto(this User user) =>
            new()
			{
				Id = user.Id,
				Name = user.Name,
				Email = user.Email
			};

		public static ReadUserDto MapToReadDto(this User user) =>
			new()
			{
				Id = user.Id,
				Name = user.Name,
				Email = user.Email,
				IsActive = user.IsActive
			};
	}
}

