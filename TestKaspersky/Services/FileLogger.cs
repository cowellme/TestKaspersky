using System.Text;

namespace TestKaspersky.Services;

public sealed class FileLogger(string categoryName, TestLogger provider) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var line = new StringBuilder()
            .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"))
            .Append(" [").Append(logLevel).Append("] [").Append(categoryName).Append("] ")
            .Append(formatter(state, exception));

        if (exception is not null)
        {
            line.AppendLine().Append(exception);
        }

        try
        {
            provider.Write(line.ToString());
        }
        catch
        {
            // ignore
        }
    }
}