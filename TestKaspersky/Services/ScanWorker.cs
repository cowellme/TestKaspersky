using TestKaspersky.Models;

namespace TestKaspersky.Services
{
    public class ScanWorker(
        BackgroundTaskQueue queue,
        IScanService scanService,
        IVulnerabilityService vulnerabilityService, 
        ICodeScanner scanner,
        ScanCancellationRegistry cancellations,
        ILogger<ScanWorker> logger,
        IConfiguration configuration)
        : BackgroundService
    {
        private readonly int _maxParrallelism = configuration.GetValue("Scanning:MaxParallel", 2);

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            await Parallel.ForEachAsync(
            queue.ReadAllAsync(ct),
            new ParallelOptions
            {
                MaxDegreeOfParallelism = _maxParrallelism,
                CancellationToken = ct
            }, async (scanId, ctx) => await ProcessAsync(scanId, ctx));
        }

        private async Task ProcessAsync(Guid scanId, CancellationToken ct)
        {
            var task = await scanService.GetById(scanId, ct);
            if (task is null || task.Status == ScanStatus.Cancelled) return;

            var token = cancellations.Registry(scanId, ct);


            try
            {
                var dateTimeNow = DateTime.UtcNow;

                task.Status = ScanStatus.InProgress;
                task.StartedAt = dateTimeNow;
                task.UpdatedAt = dateTimeNow;

                await scanService.UpdateAsync(task, token);

                var found = await scanner.ScanAsync(task, token);

                if (found is { Count: > 0 })
                {
                    found.ForEach(v =>
                    {
                        var id = Guid.NewGuid();
                        v.Id = id;
                        v.ScanTaskId = scanId;
                        v.CreatedAt = DateTime.UtcNow;

                        task.VulnerabilityIds.Add(id);
                    });
                    await vulnerabilityService.AddRange(found, token);
                }

                if (task.Status != ScanStatus.Cancelled)
                {
                    task.Status = token.IsCancellationRequested ? ScanStatus.Cancelled : ScanStatus.Completed;
                    task.CompletedAt = DateTime.UtcNow;
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested || token.IsCancellationRequested)
            {
                task.Status = ScanStatus.Cancelled;
                task.ErrorMessage = "Отменено";
            }
            catch (Exception ex)
            {
                task.Status = ScanStatus.Failed;
                task.ErrorMessage = ex.Message;
                logger.LogError(ex, "Ошибка скана: {ScanId}", scanId);
            }
            finally
            {
                cancellations.Unregistr(scanId);
            }

            task.UpdatedAt = DateTime.UtcNow;
            await scanService.UpdateAsync(task, ct);
        }
    }


}
