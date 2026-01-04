using ExcelWebApp2.Repositories;

namespace ExcelWebApp2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var builder = WebApplication.CreateBuilder(args);
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot"),
                ContentRootPath = AppContext.BaseDirectory
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer(); // Required for Swagger
            builder.Services.AddScoped<FileReaderRepository>();
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

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.UseDefaultFiles(); 
            app.UseStaticFiles();

            app.MapFallbackToFile("index.html");


            app.MapControllers();
            app.Run();
        }
    }
}
