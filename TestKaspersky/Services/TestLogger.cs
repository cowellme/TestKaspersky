using System.Text;

namespace TestKaspersky.Services
{
    public class TestLogger : ILoggerProvider
    {
        private readonly StreamWriter _writer;
        private readonly Lock _lock = new();

        public TestLogger(string path)
        {
            var fullPath = Path.GetFullPath(path);
            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _writer = new StreamWriter(
                new FileStream(fullPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite),
                Encoding.UTF8)
            {
                AutoFlush = true
            };
        }

        public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, this);

        internal void Write(string line)
        {
            lock (_lock)
            {
                _writer.WriteLine(line);
            }
        }

        public void Dispose() => _writer.Dispose();
    }
}
