using Microsoft.EntityFrameworkCore;
using TestKaspersky.Models;

namespace TestKaspersky.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Vulnerability> Vulnerabilities => Set<Vulnerability>();
        public DbSet<ScanTask> ScanTasks => Set<ScanTask>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vulnerability>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Content).HasMaxLength(1000);
            });

            modelBuilder.Entity<ScanTask>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }
}
