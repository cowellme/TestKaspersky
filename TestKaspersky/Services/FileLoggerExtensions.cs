namespace TestKaspersky.Services;

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string path)
    {
        builder.AddProvider(new TestLogger(path));
        return builder;
    }
}