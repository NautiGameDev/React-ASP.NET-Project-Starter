using BaseAccountSystem.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BaseAccountSystem.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<Account>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = "d3c359f6-46d0-4201-8c24-4ddc89ef2433",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "d3c359f6-46d0-4201-8c24-4ddc89ef2433"
                },
                new IdentityRole
                {
                    Id = "0802b7cb-a06e-405c-b6e9-6bf1b516e33e",
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = "0802b7cb-a06e-405c-b6e9-6bf1b516e33e"
                }
            };

            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}
