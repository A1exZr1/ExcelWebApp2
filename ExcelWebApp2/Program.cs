using ExcelWebApp2.Repositories;
using ExcelWebApp2.Repositories.Processing;

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
            builder.Services.AddEndpointsApiExplorer(); // Required for Swagger
            builder.Services.AddScoped<FileReaderRepository>();
            builder.Services.AddSingleton<IOzonV1Processor, OzonV1Processor>();
            builder.Services.AddSingleton<IOzonV2Processor, OzonV2Processor>();
            builder.Services.AddSingleton<IWildberriesProcessor, WildberriesProcessor>();
            builder.Services.AddSingleton<ProcessorRepository>();
            builder.Services.AddScoped<ExcelExportService>();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SupportNonNullableReferenceTypes();
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExcelWebApp2 v1");
                    c.RoutePrefix = "swagger";
                });
            }

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
