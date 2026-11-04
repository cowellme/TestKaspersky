using TestKaspersky.Data;
using TestKaspersky.Models;

namespace TestKaspersky.Tests;

public class InMemoryRepositoryTests
{
    private readonly InMemoryRepository<Vulnerability, Guid> _repository = new();

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsEntity()
    {
        var vulnerability = new Vulnerability
        {
            Id = Guid.NewGuid(),
            Title = "SQL-инъекция",
            Severity = SeverityLevel.High,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(vulnerability);
        var result = await _repository.GetByIdAsync(vulnerability.Id);

        Assert.NotNull(result);
        Assert.Equal(vulnerability.Title, result.Title);
        Assert.Equal(SeverityLevel.High, result.Severity);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ExistingEntity_RemovesIt()
    {
        var vulnerability = new Vulnerability { Id = Guid.NewGuid(), Title = "t", Severity = SeverityLevel.Low };

        await _repository.AddAsync(vulnerability);
        var deleted = await _repository.DeleteAsync(vulnerability.Id);
        var afterDelete = await _repository.GetByIdAsync(vulnerability.Id);

        Assert.True(deleted);
        Assert.Null(afterDelete);
    }

    [Fact]
    public async Task AddRangeAsync_StoresAllEntities()
    {
        var entities = Enumerable.Range(0, 5).Select(i => new Vulnerability
        {
            Id = Guid.NewGuid(),
            Title = $"v{i}",
            Severity = SeverityLevel.Medium
        }).ToList();

        await _repository.AddRangeAsync(entities);
        var all = await _repository.GetAllAsync();

        Assert.Equal(5, all.Count);
    }
}