using ExcelWebApp2.Repositories;
using ExcelWebApp2.Repositories.Processing;
using ExcelWebApp2.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ExcelWebApp2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var webRoot = builder.Environment.WebRootPath
                ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot");

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddHttpContextAccessor();
            var defaultConnectionString = GetDefaultConnectionString(builder.Configuration);
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseNpgsql(
                        defaultConnectionString,
                        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history"))
                    .UseSnakeCaseNamingConvention());
            builder.Services
                .AddIdentity<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "excelwebapp.auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.SlidingExpiration = true;
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }
                };
            });
            builder.Services.AddScoped<FileReaderRepository>();
            builder.Services.AddSingleton<IOzonV1Processor, OzonV1Processor>();
            builder.Services.AddSingleton<IOzonV2Processor, OzonV2Processor>();
            builder.Services.AddSingleton<IWildberriesProcessor, WildberriesProcessor>();
            builder.Services.AddSingleton<IProcessingSessionStore, ProcessingSessionStore>();
            builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
            builder.Services.AddScoped<ProcessorRepository>();
            builder.Services.AddScoped<ExcelExportService>();
            builder.Services.AddOpenApiDocument(options =>
            {
                options.DocumentName = "v1";
                options.Title = "ExcelWebApp2 API";
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseOpenApi();
                app.UseSwaggerUi(settings =>
                {
                    settings.Path = "/swagger";
                    settings.DocumentPath = "/swagger/v1/swagger.json";
                });
            }

            app.UseMiddleware<ApiExceptionMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            IdentitySeeder.SeedAdminUserAsync(
                app.Services,
                app.Configuration,
                app.Logger).GetAwaiter().GetResult();

            app.UseDefaultFiles();
            Console.WriteLine($"[INFO] Static files path: {webRoot}");
            app.UseStaticFiles();
            app.MapControllers();
            app.MapFallbackToFile("index.html");
            app.Run();
        }

        private static string GetDefaultConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
            }

            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            if (!ConnectionStringContainsKey(connectionString, "Timeout", "Connection Timeout"))
            {
                builder.Timeout = 3;
            }

            return builder.ConnectionString;
        }

        private static bool ConnectionStringContainsKey(string connectionString, params string[] keys)
        {
            var keySet = keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var separatorIndex = part.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var key = part[..separatorIndex].Trim();
                if (keySet.Contains(key))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
