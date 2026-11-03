using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
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
            builder.Services.AddSwaggerGen();

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
                
            }
            
            
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
