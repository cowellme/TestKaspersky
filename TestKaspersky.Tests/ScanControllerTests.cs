using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using TestKaspersky.Controllers;
using TestKaspersky.Models;

namespace TestKaspersky.Tests;

public class ScanControllerTests
{
    private static ScanController CreateController(FakeScanService service)
        => new(service, NullLogger<ScanController>.Instance);

    [Fact]
    public async Task GetAll_EmptyStore_ReturnsNoContent()
    {
        var controller = CreateController(new FakeScanService());

        var result = await controller.GetAll(CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetAll_WithTasks_ReturnsOkWithList()
    {
        var task = new ScanTask
        {
            Id = Guid.NewGuid(),
            RepositoryUrl = "https://gitlab.com/org/repo",
            Status = ScanStatus.Queued
        };
        var service = new FakeScanService
        {
            GetAllImpl = _ => Task.FromResult<IReadOnlyList<ScanTask>>([task])
        };
        var controller = CreateController(service);

        var result = await controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IReadOnlyList<ScanTask>>(ok.Value);
        Assert.Single(list);
        Assert.Equal(task.Id, list[0].Id);
    }

    [Fact]
    public async Task GetAll_ServiceThrows_ReturnsBadRequest()
    {
        var service = new FakeScanService
        {
            GetAllImpl = _ => throw new InvalidOperationException("Хранилище упало")
        };
        var controller = CreateController(service);

        var result = await controller.GetAll(CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }


    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        var controller = CreateController(new FakeScanService());

        var result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOkWithTask()
    {
        var task = new ScanTask
        {
            Id = Guid.NewGuid(),
            RepositoryUrl = "https://gitlab.com/org/repo",
            Status = ScanStatus.Completed
        };
        var service = new FakeScanService
        {
            GetByIdImpl = (_, _) => Task.FromResult<ScanTask?>(task)
        };
        var controller = CreateController(service);

        var result = await controller.GetById(task.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(task, ok.Value);
    }


    [Fact]
    public async Task ScanUrl_CreatesTask_ReturnsOkWithTaskId_AndPassesUrl()
    {
        var expectedId = Guid.NewGuid();
        string? passedUrl = null;
        var service = new FakeScanService
        {
            CreateScanImpl = (url, _) =>
            {
                passedUrl = url;
                return Task.FromResult<ScanTask?>(new ScanTask
                {
                    Id = expectedId,
                    RepositoryUrl = url,
                    Status = ScanStatus.Queued
                });
            }
        };
        var controller = CreateController(service);

        var result = await controller.ScanUrl("https://gitlab.com/org/repo", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedId, ok.Value);
        Assert.Equal("https://gitlab.com/org/repo", passedUrl);
    }

    [Fact]
    public async Task ScanUrl_ServiceReturnsNull_ReturnsNotFound()
    {
        var controller = CreateController(new FakeScanService());   // дефолт возвращает null

        var result = await controller.ScanUrl("https://gitlab.com/org/repo", CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ScanUrl_ServiceThrows_ReturnsBadRequest()
    {
        var service = new FakeScanService
        {
            CreateScanImpl = (_, _) => throw new InvalidOperationException("Очередь недоступна")
        };
        var controller = CreateController(service);

        var result = await controller.ScanUrl("https://gitlab.com/org/repo", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }


    [Fact]
    public async Task Cancel_UnknownTask_ReturnsNotFound()
    {
        var controller = CreateController(new FakeScanService());

        var result = await controller.Cancel(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Cancel_Success_ReturnsOkWithResponse()
    {
        var id = Guid.NewGuid();
        var response = new CancelScanResponse
        {
            ScanId = id,
            Success = true,
            CancelledAt = DateTime.UtcNow
        };
        var service = new FakeScanService
        {
            CancelScanImpl = (_, _) => Task.FromResult<CancelScanResponse?>(response)
        };
        var controller = CreateController(service);

        var result = await controller.Cancel(id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<CancelScanResponse>(ok.Value);
        Assert.True(body.Success);
        Assert.Equal(id, body.ScanId);
        Assert.NotEqual(default, body.CancelledAt);
    }

    [Fact]
    public async Task Cancel_AlreadyFinished_ReturnsOkWithSuccessFalse()
    {
        var response = new CancelScanResponse
        {
            ScanId = Guid.NewGuid(),
            Success = false,
            CancelledAt = DateTime.UtcNow
        };
        var service = new FakeScanService
        {
            CancelScanImpl = (_, _) => Task.FromResult<CancelScanResponse?>(response)
        };
        var controller = CreateController(service);

        var result = await controller.Cancel(response.ScanId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<CancelScanResponse>(ok.Value);
        Assert.False(body.Success);
    }
}