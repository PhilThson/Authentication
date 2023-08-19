using System.ComponentModel.DataAnnotations;

namespace Authentication.Domain.DTOs.Create
{
    public record CreateUserDto
	{
        [Required]
        [StringLength(64)]
        public string? Name { get; set; }

        [Required]
        [StringLength(128)]
        public string? Email { get; set; }

        [Required]
        [StringLength(32)]
        public string? Password { get; set; }
    }
}

