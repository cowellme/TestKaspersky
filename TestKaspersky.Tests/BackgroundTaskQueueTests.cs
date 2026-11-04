using Microsoft.Extensions.Configuration;
using TestKaspersky.Services;

namespace TestKaspersky.Tests;

public class BackgroundTaskQueueTests
{
    private static BackgroundTaskQueue CreateQueue(int capacity = 2) => new(
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BackgroundQueue:Capacity"] = capacity.ToString()
            })
            .Build());

    [Fact]
    public async Task EnqueueThenRead_ReturnsSameId()
    {
        var queue = CreateQueue();
        var id = Guid.NewGuid();

        await queue.EnqueueAsync(id);
        await using var enumerator = queue.ReadAllAsync().GetAsyncEnumerator();

        Assert.True(await enumerator.MoveNextAsync());
        Assert.Equal(id, enumerator.Current);
    }

    [Fact]
    public async Task PendingCount_ReflectsQueuedItems_AndIsFull()
    {
        var queue = CreateQueue(capacity: 2);
        await queue.EnqueueAsync(Guid.NewGuid());
        await queue.EnqueueAsync(Guid.NewGuid());

        Assert.Equal(2, queue.PendingCount);
        Assert.True(queue.IsFull);
    }
}