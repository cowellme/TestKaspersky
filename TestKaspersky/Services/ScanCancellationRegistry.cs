using System.Collections.Concurrent;

namespace TestKaspersky.Services
{
    public class ScanCancellationRegistry(ILogger<ScanCancellationRegistry> logger)
    {
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _sources = new ConcurrentDictionary<Guid, CancellationTokenSource>();

        public CancellationToken Registry(Guid scanId, CancellationToken token)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            _sources.TryAdd(scanId, cts);
            return cts.Token;
        }

        public bool TryCancel(Guid scanId)
        {
            if (!_sources.TryGetValue(scanId, out var cts)) 
                return false;

            try
            {
                cts.Cancel();
                return true;
            }
            catch (ObjectDisposedException ex)
            {
                logger.LogWarning(ex.Message);
                return false;
            }
        }

        public void Unregistr(Guid scanId)
        {
            if (_sources.TryRemove(scanId, out var cts)) 
                cts.Dispose();
        }
    }
}
