using TestKaspersky.Data;
using TestKaspersky.Models;

namespace TestKaspersky.Services
{
    public interface IScanService
    {
        public Task<IReadOnlyList<ScanTask>> GetAll(CancellationToken ct);
        public Task<ScanTask?> CreateScan(string url, CancellationToken ct); 
        public Task<ScanTask?> GetById(Guid id, CancellationToken ct);
        public Task UpdateAsync(ScanTask task, CancellationToken ct);
        public Task<CancelScanResponse?> CancelScan(Guid id, CancellationToken ct); 
    }

    public class ScanService(IRepository<ScanTask, 
        Guid> repository,
        ScanCancellationRegistry cancellations,
        BackgroundTaskQueue queue, 
        ILogger<ScanService> logger) : IScanService
    {
        public async Task<IReadOnlyList<ScanTask>> GetAll(CancellationToken ct) => await repository.GetAllAsync(ct);
        public async Task<ScanTask?> CreateScan(string url, CancellationToken ct)
        {
            try
            {
                var task = new ScanTask
                {
                    Id = Guid.NewGuid(),
                    RepositoryUrl = url,
                    Status = ScanStatus.Queued,
                    CreatedAt = DateTime.UtcNow
                };
                await repository.AddAsync(task, ct);
                await queue.EnqueueAsync(task.Id, ct);
                return task;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message);
                return null;
            }
        }
        public async Task<ScanTask?> GetById(Guid id, CancellationToken ct) => await repository.GetByIdAsync(id, ct);
        public async Task UpdateAsync(ScanTask task, CancellationToken ct) => await repository.UpdateAsync(task, ct);
        public async Task<CancelScanResponse?> CancelScan(Guid id, CancellationToken ct)
        {
            var task = await repository.GetByIdAsync(id, ct);

            if (task is null) return null;

            var dateTimeNow = DateTime.UtcNow;

            switch (task.Status)
            {
                case ScanStatus.Queued:
                    return await CloseAndUpdate();

                case ScanStatus.InProgress:
                    var result = cancellations.TryCancel(id);
                    if (!result)
                        logger.LogWarning("Не удалось прервать задачу");
                    return await CloseAndUpdate(result);

                default:
                    return await CloseAndUpdate(false);

            }

            async Task<CancelScanResponse> CloseAndUpdate(bool success = true)
            {
                if (success)
                {
                    task.Status = ScanStatus.Cancelled;
                    task.UpdatedAt = dateTimeNow;
                    await repository.UpdateAsync(task, ct);
                }
                
                return new CancelScanResponse
                {
                    ScanId = id,
                    Success = success,
                    CancelledAt = dateTimeNow
                };
            }
        }
    }
}
