using Authentication.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Infrastructure.DataAccess
{
    public class AuthDbContext : DbContext
	{
		public AuthDbContext(DbContextOptions<AuthDbContext> options)
			: base(options)
		{

		}

		public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(builder =>
            {
                builder.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}

