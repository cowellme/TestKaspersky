using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TestKaspersky.Data;
using TestKaspersky.Models;
using TestKaspersky.Services;

namespace TestKaspersky.Tests;

public class ScanServiceTests
{
    private static ScanService CreateService(
        out InMemoryRepository<ScanTask, Guid> tasks,
        out ScanCancellationRegistry registry,
        out BackgroundTaskQueue queue)
    {
        tasks = new InMemoryRepository<ScanTask, Guid>();
        registry = new ScanCancellationRegistry(NullLogger<ScanCancellationRegistry>.Instance);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BackgroundQueue:Capacity"] = "10"
            })
            .Build();
        queue = new BackgroundTaskQueue(config);

        return new ScanService(tasks, registry, queue, NullLogger<ScanService>.Instance);
    }

    [Fact]
    public async Task CreateScanAsync_ReturnsQueuedTaskWithId()
    {
        var ct = CancellationToken.None;
        var service = CreateService(out var tasks, out _, out _);

        var task = await service.CreateScan("https://gitlab.com/org/repo", ct);

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal(ScanStatus.Queued, task.Status);
        Assert.Equal("https://gitlab.com/org/repo", task.RepositoryUrl);
        Assert.NotNull(await tasks.GetByIdAsync(task.Id, ct));
    }

    [Fact]
    public async Task CancelAsync_QueuedTask_SetsCancelled()
    {
        var ct = CancellationToken.None;
        var service = CreateService(out var tasks, out _, out _);
        var task = await service.CreateScan("https://gitlab.com/org/repo", ct);

        var result = await service.CancelScan(task.Id, ct);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(ScanStatus.Cancelled, (await tasks.GetByIdAsync(task.Id))!.Status);
    }

    [Fact]
    public async Task CancelAsync_CompletedTask_ReturnsFalse()
    {

        var ct = CancellationToken.None;
        var service = CreateService(out var tasks, out _, out _);
        var task = new ScanTask
        {
            Id = Guid.NewGuid(),
            RepositoryUrl = "https://gitlab.com/org/repo",
            Status = ScanStatus.Completed
        };
        await tasks.AddAsync(task);

        var result = await service.CancelScan(task.Id, ct);

        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task CancelAsync_UnknownTask_ReturnsNull()
    {
        var ct = CancellationToken.None;
        var service = CreateService(out var tasks, out _, out _);

        var result = await service.CancelScan(Guid.NewGuid(), ct);

        Assert.Null(result);
    }


}