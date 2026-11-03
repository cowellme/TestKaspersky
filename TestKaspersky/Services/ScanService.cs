using TestKaspersky.Data;
using TestKaspersky.Models;

namespace TestKaspersky.Services
{
    public interface IScanService
    {
        public Task<IReadOnlyList<ScanTask>> GetAll(CancellationToken ct);
        public Task<ScanTask> CreateScan(string url, CancellationToken ct); 
        public Task<ScanTask?> GetById(Guid id, CancellationToken ct);
    }

    public class ScanService(IRepository<ScanTask, Guid> scanTasks, BackgroundTaskQueue queue) : IScanService
    {
        public async Task<IReadOnlyList<ScanTask>> GetAll(CancellationToken ct) => await scanTasks.GetAllAsync(ct);

        public async Task<ScanTask> CreateScan(string url, CancellationToken ct)
        {
            var task = new ScanTask
            {
                Id = Guid.NewGuid(),
                RepositoryUrl = url,
                Status = ScanStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            await scanTasks.AddAsync(task, ct);
            await queue.EnqueueAsync(task.Id, ct);
            return task;
        }

        public async Task<ScanTask?> GetById(Guid id, CancellationToken ct) => await scanTasks.GetByIdAsync(id, ct);
    }
}
