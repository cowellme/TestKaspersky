using Microsoft.Extensions.Logging.Abstractions;
using TestKaspersky.Models;
using TestKaspersky.Services;

namespace TestKaspersky.Tests;

public class CodeScannerTests
{
    [Fact]
    public async Task ScanAsync_PrecancelledToken_ThrowsOperationCanceled()
    {
        var task = new ScanTask { RepositoryUrl = "https://gitlab.com/org/repo" };
        var scanner = new CodeScanner(NullLogger<CodeScanner>.Instance);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => scanner.ScanAsync(task, cts.Token));
    }

    [Fact]
    public async Task ScanAsync_ReturnsListOfVulnerabilities()
    {
        var task = new ScanTask { RepositoryUrl = "https://gitlab.com/org/repo" };
        var scanner = new CodeScanner(NullLogger<CodeScanner>.Instance);

        var result = await scanner.ScanAsync(task, CancellationToken.None, 1);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}