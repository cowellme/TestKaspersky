using TestKaspersky.Models;
using TestKaspersky.Services;

namespace TestKaspersky.Tests;

public class FakeScanService : IScanService
{
    public Func<CancellationToken, Task<IReadOnlyList<ScanTask>>> GetAllImpl { get; set; } =
        _ => Task.FromResult<IReadOnlyList<ScanTask>>([]);

    public Func<string, CancellationToken, Task<ScanTask?>> CreateScanImpl { get; set; } =
        (_, _) => Task.FromResult<ScanTask?>(null);

    public Func<Guid, CancellationToken, Task<ScanTask?>> GetByIdImpl { get; set; } =
        (_, _) => Task.FromResult<ScanTask?>(null);

    public Func<ScanTask, CancellationToken, Task> UpdateImpl { get; set; } =
        (_, _) => Task.CompletedTask;

    public Func<Guid, CancellationToken, Task<CancelScanResponse?>> CancelScanImpl { get; set; } =
        (_, _) => Task.FromResult<CancelScanResponse?>(null);

    public Task<IReadOnlyList<ScanTask>> GetAll(CancellationToken ct) => GetAllImpl(ct);
    public Task<ScanTask?> CreateScan(string url, CancellationToken ct) => CreateScanImpl(url, ct);
    public Task<ScanTask?> GetById(Guid id, CancellationToken ct) => GetByIdImpl(id, ct);
    public Task UpdateAsync(ScanTask task, CancellationToken ct) => UpdateImpl(task, ct);
    public Task<CancelScanResponse?> CancelScan(Guid id, CancellationToken ct) => CancelScanImpl(id, ct);
}