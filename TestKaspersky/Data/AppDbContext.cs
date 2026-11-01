using Microsoft.EntityFrameworkCore;

namespace TestKaspersky.Data
{
    /// <summary>
    /// Контекст базы данных PostgreSQL.
    /// </summary>
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<WeatherForecast> WeatherForecasts => Set<WeatherForecast>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WeatherForecast>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Summary).HasMaxLength(200);
            });
        }
    }
}
