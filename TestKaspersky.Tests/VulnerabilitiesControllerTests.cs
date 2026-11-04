using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using TestKaspersky.Controllers;
using TestKaspersky.Data;
using TestKaspersky.Models;
using TestKaspersky.Services;

namespace TestKaspersky.Tests;

public class VulnerabilitiesControllerTests
{
    private readonly VulnerabilitiesController _controller = new(
        new VulnerabilityService(new InMemoryRepository<Vulnerability, Guid>()),
        NullLogger<VulnerabilitiesController>.Instance);

    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        var result = await _controller.GetById(Guid.NewGuid(), CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsIdOfStoredVulnerability()
    {
        var result = await _controller.Create(new CreateVulnerabilityRequest
        {
            Title = "SQL-инъекция",
            Severity = SeverityLevel.High
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Guid>(ok.Value);
    }
}