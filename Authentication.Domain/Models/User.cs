using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authentication.Domain.Models
{
    public class User
	{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(64)]
        public string Name { get; set; }

        [StringLength(128)]
        public string Email { get; set; }

        [StringLength(512)]
        public string PasswordHash { get; set; }

        [StringLength(512)]
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiration { get; set; }

        public bool RefreshTokenIsRevoked { get =>
            RefreshTokenExpiration <= DateTime.Now; }

        public bool IsActive { get; set; } = true;
    }
}

