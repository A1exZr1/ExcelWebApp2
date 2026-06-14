using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExcelWebApp2.Infrastructure
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().ToTable("asp_net_users");
            builder.Entity<IdentityRole>().ToTable("asp_net_roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("asp_net_user_roles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("asp_net_user_claims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("asp_net_user_logins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("asp_net_role_claims");
            builder.Entity<IdentityUserToken<string>>().ToTable("asp_net_user_tokens");
        }
    }
}