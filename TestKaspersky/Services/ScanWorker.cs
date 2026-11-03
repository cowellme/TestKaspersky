using System.Net.WebSockets;
using TestKaspersky.Data;
using TestKaspersky.Models;

namespace TestKaspersky.Services
{
    public class ScanWorker(BackgroundTaskQueue queue, IServiceScopeFactory scopeFactory, ILogger<ScanWorker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            await foreach (var scanId in queue.ReadAllAsync(ct))
            {
                await ProcessAsync(scanId, ct);
            }
        }

        private async Task ProcessAsync(Guid scanId, CancellationToken ct)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var scanner = scope.ServiceProvider.GetRequiredService<ICodeScanner>();
            var vulnerabilityService = scope.ServiceProvider.GetRequiredService<IVulnerabilityService>();

            var task = await db.ScanTasks.FindAsync([scanId], ct);
            if (task is null) return;

            try
            {
                task.Status = ScanStatus.InProgress;
                task.StartedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);

                
                var found = await scanner.ScanAsync(task.RepositoryUrl, ct);
                
                db.Vulnerabilities.AddRange(found);

                task.Status = ScanStatus.Completed;
                task.CompletedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                task.Status = ScanStatus.Failed;
                task.ErrorMessage = ex.Message;
                logger.LogError(ex, "Скан {ScanId} упал", scanId);
            }
            await db.SaveChangesAsync(ct);
        }
    }

    public interface ICodeScanner
    {
        Task<List<Vulnerability>> ScanAsync(string url, CancellationToken ct);
    }

    public class CodeScanner : ICodeScanner
    {
        public async Task<List<Vulnerability>> ScanAsync(string url, CancellationToken ct)
        {
            var rndCountVulnerability = new Random().Next(1, 10);
            var listVulnerabilities = new List<Vulnerability>();
            for (var i = 0; i < rndCountVulnerability; i++)
            {
                await Task.Delay(5000, ct);
                var rndSeverityLevel = new Random().Next(0, 3);
                listVulnerabilities.Add(new Vulnerability
                {
                    Title = $"test_{i}",
                    Severity = Enum.GetValues<SeverityLevel>()[rndSeverityLevel],
                });
            }

            return listVulnerabilities;
        }
    }
}
