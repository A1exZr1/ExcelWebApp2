using ExcelWebApp2.Repositories;
using ExcelWebApp2.Repositories.Processing;
using ExcelWebApp2.Infrastructure;

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
            builder.Services.AddScoped<FileReaderRepository>();
            builder.Services.AddSingleton<IOzonV1Processor, OzonV1Processor>();
            builder.Services.AddSingleton<IOzonV2Processor, OzonV2Processor>();
            builder.Services.AddSingleton<IWildberriesProcessor, WildberriesProcessor>();
            builder.Services.AddSingleton<ProcessorRepository>();
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
            app.UseAuthorization();

            app.UseDefaultFiles();
            Console.WriteLine($"[INFO] Static files path: {webRoot}");
            app.UseStaticFiles();
            app.MapControllers();
            app.MapFallbackToFile("index.html");
            app.Run();
        }
    }
}
