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

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Не задана строка подключения ConnectionStrings:DefaultConnection");

            builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

            builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            builder.Services.AddScoped<IVulnerabilityService, VulnerabilityService>();

            var app = builder.Build();

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
