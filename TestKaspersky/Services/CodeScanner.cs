using TestKaspersky.Models;

namespace TestKaspersky.Services;

public interface ICodeScanner
{
    Task<List<Vulnerability>?> ScanAsync(ScanTask task, CancellationToken ct);
}
public class CodeScanner(ILogger<CodeScanner> logger) : ICodeScanner
{
    public async Task<List<Vulnerability>?> ScanAsync(ScanTask task, CancellationToken ct)
    {
        logger.LogInformation("Старт сканирования");

        var rndCount = Random.Shared.Next(1, 10);
        var list = new List<Vulnerability>();

        for (var i = 0; i < rndCount; i++)
        {
            ct.ThrowIfCancellationRequested();

            logger.LogInformation("Найдена уязвимость по ссылке: {Url}!", task.RepositoryUrl);
            await Task.Delay(5000, ct);
            var rndSeverityLevel = new Random().Next(0, 3);
            list.Add(new Vulnerability
            {
                Title = $"test_{i}",
                Severity = Enum.GetValues<SeverityLevel>()[rndSeverityLevel],
            });
        }
        logger.LogInformation("Сканирование закончено");
        return list;
    }
}