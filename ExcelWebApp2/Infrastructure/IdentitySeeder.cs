using Microsoft.AspNetCore.Identity;

namespace ExcelWebApp2.Infrastructure
{
    public static class IdentitySeeder
    {
        public static async Task SeedAdminUserAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
        {
            if (!configuration.GetValue("SeedAdmin:Enabled", false))
            {
                return;
            }

            var email = configuration["SeedAdmin:Email"];
            var password = configuration["SeedAdmin:Password"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("SeedAdmin is enabled, but email or password is empty.");
                return;
            }

            try
            {
                using var scope = services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (!await dbContext.Database.CanConnectAsync())
                {
                    logger.LogWarning("Seed admin user skipped because the database is not available.");
                    return;
                }

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var role = configuration["SeedAdmin:Role"] ?? "admin";

                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join("; ", roleResult.Errors.Select(x => x.Description));
                        logger.LogWarning("Failed to seed admin role: {Errors}", errors);
                        return;
                    }
                }

                var existingUser = await userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    if (!await userManager.IsInRoleAsync(existingUser, role))
                    {
                        await userManager.AddToRoleAsync(existingUser, role);
                    }

                    return;
                }

                var user = new ApplicationUser
                {
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(x => x.Description));
                    logger.LogWarning("Failed to seed admin user: {Errors}", errors);
                    return;
                }

                var addToRoleResult = await userManager.AddToRoleAsync(user, role);
                if (!addToRoleResult.Succeeded)
                {
                    var errors = string.Join("; ", addToRoleResult.Errors.Select(x => x.Description));
                    logger.LogWarning("Failed to add seed admin user to role: {Errors}", errors);
                    return;
                }

                logger.LogInformation("Seed admin user was created: {Email}", email);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to seed admin user. Check that the database is available and migrations are applied.");
            }
        }
    }
}
