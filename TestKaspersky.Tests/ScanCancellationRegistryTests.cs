using Microsoft.Extensions.Logging.Abstractions;
using TestKaspersky.Services;

namespace TestKaspersky.Tests;

public class ScanCancellationRegistryTests
{
    private readonly ScanCancellationRegistry _registry =
        new(NullLogger<ScanCancellationRegistry>.Instance);

    [Fact]
    public void TryCancel_RegisteredToken_CancelsIt()
    {
        var scanId = Guid.NewGuid();
        var token = _registry.Registry(scanId, CancellationToken.None);

        var cancelled = _registry.TryCancel(scanId);

        Assert.True(cancelled);
        Assert.True(token.IsCancellationRequested);
    }

    [Fact]
    public void TryCancel_UnknownId_ReturnsFalse()
    {
        Assert.False(_registry.TryCancel(Guid.NewGuid()));
    }

    [Fact]
    public void TryCancel_AfterUnregister_ReturnsFalse()
    {
        var scanId = Guid.NewGuid();
        _registry.Registry(scanId, CancellationToken.None);
        _registry.Unregistr(scanId);

        Assert.False(_registry.TryCancel(scanId));
    }
}