using System.Reflection;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using TestKaspersky.Data;
using TestKaspersky.Services;

namespace TestKaspersky
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddFile(Path.Combine("logs", "app.log"));

            builder.Services.AddHttpLogging(options =>
            {
                options.LoggingFields =
                    HttpLoggingFields.RequestMethod
                    | HttpLoggingFields.RequestPath
                    | HttpLoggingFields.ResponseStatusCode
                    | HttpLoggingFields.Duration;
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Vulnerability Tracker API",
                    Version = "v1",
                    Description = "Сервис управления уязвимостями ПО: CRUD уязвимостей " +
                                  "и асинхронные задачи сканирования репозиториев."
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
            });

            builder.Services.AddSingleton(typeof(IRepository<,>), typeof(InMemoryRepository<,>));
            builder.Services.AddSingleton<IVulnerabilityService, VulnerabilityService>(); 
            builder.Services.AddSingleton<ScanCancellationRegistry>();
            builder.Services.AddSingleton<IScanService, ScanService>();
            builder.Services.AddSingleton<BackgroundTaskQueue>();
            builder.Services.AddSingleton<ICodeScanner, CodeScanner>();
            builder.Services.AddHostedService<ScanWorker>();
            var app = builder.Build();

            app.UseHttpLogging();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
