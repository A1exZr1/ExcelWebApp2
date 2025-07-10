using ExcelWebApp2.Repositories;

namespace ExcelWebApp2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
