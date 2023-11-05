using System.ComponentModel.DataAnnotations;

namespace Authentication.Domain.DTOs.Common
{
    public class AuthenticateRequestDto
	{
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}

