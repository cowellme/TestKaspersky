using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using TestKaspersky.Data;
using TestKaspersky.Models;
using TestKaspersky.Services;

namespace TestKaspersky.Tests;

public class ScanWorkerTests
{
    private static (ScanWorker Worker, ScanService Service, InMemoryRepository<ScanTask, Guid> Tasks,
        InMemoryRepository<Vulnerability, Guid> Vulns) CreateWorker(ICodeScanner scanner)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BackgroundQueue:Capacity"] = "10",
                ["Scanning:MaxParallel"] = "2"
            })
            .Build();

        var tasks = new InMemoryRepository<ScanTask, Guid>();
        var vulns = new InMemoryRepository<Vulnerability, Guid>();
        var registry = new ScanCancellationRegistry(NullLogger<ScanCancellationRegistry>.Instance);
        var queue = new BackgroundTaskQueue(config);
        var scanService = new ScanService(tasks, registry, queue, NullLogger<ScanService>.Instance);
        var vulnService = new VulnerabilityService(vulns);
        var worker = new ScanWorker(queue, scanService, vulnService, scanner, registry,
            NullLogger<ScanWorker>.Instance, config);

        return (worker, scanService, tasks, vulns);
    }

    [Fact]
    public async Task ProcessAsync_QueuedTask_CompletesAndSavesVulnerabilities()
    {
        var (worker, service, tasks, vulns) = CreateWorker(new FakeScanner());
        var task = await service.CreateScan("https://gitlab.com/org/repo", CancellationToken.None);
        Assert.NotNull(task);

        await worker.StartAsync(CancellationToken.None);
        var current = await WaitForStatus(tasks, task.Id,
            ScanStatus.Completed, ScanStatus.Failed);
        await worker.StopAsync(CancellationToken.None);

        Assert.Equal(ScanStatus.Completed, current.Status);
        Assert.NotNull(current.CompletedAt);
        Assert.Equal(3, (await vulns.GetAllAsync()).Count);
        Assert.All(await vulns.GetAllAsync(), v => Assert.Equal(task.Id, v.ScanTaskId));
    }

    [Fact]
    public async Task Cancel_RunningScan_InterruptsAndSetsCancelled()
    {
        var (worker, service, tasks, _) = CreateWorker(
            new FakeScanner { Delay = TimeSpan.FromSeconds(30) });

        var task = await service.CreateScan("https://gitlab.com/org/repo", CancellationToken.None);
        Assert.NotNull(task);

        await worker.StartAsync(CancellationToken.None);
        await WaitForStatus(tasks, task.Id, ScanStatus.InProgress);   // дождались запуска

        var cancelResult = await service.CancelScan(task.Id, CancellationToken.None);

        var current = await WaitForStatus(tasks, task.Id, ScanStatus.Cancelled);
        await worker.StopAsync(CancellationToken.None);

        Assert.NotNull(cancelResult);
        Assert.True(cancelResult.Success);
        Assert.Equal(ScanStatus.Cancelled, current.Status);
    }

    private static async Task<ScanTask> WaitForStatus(
        InMemoryRepository<ScanTask, Guid> tasks, Guid id, params ScanStatus[] statuses)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < deadline)
        {
            var task = (await tasks.GetByIdAsync(id))!;
            if (statuses.Contains(task.Status)) return task;
            await Task.Delay(20);
        }
        throw new Xunit.Sdk.XunitException(
            $"Задача {id} не перешла в ожидаемый статус за 5 секунд");
    }
}

public class FakeScanner : ICodeScanner
{
    public TimeSpan Delay { get; init; } = TimeSpan.Zero;
    public int VulnerabilitiesCount { get; init; } = 3;

    public async Task<List<Vulnerability>?> ScanAsync(ScanTask task, CancellationToken ct, int delay = 1)
    {
        if (Delay > TimeSpan.Zero)
        {
            await Task.Delay(Delay, ct);
        }

        return Enumerable.Range(0, VulnerabilitiesCount)
            .Select(i => new Vulnerability { Title = $"test_{i}", Severity = SeverityLevel.Low })
            .ToList();
    }
}