using System.Threading.Channels;

namespace TestKaspersky.Services
{
    public class BackgroundTaskQueue
    {
        private readonly Channel<Guid> _queue;
        private readonly int _capacity;

        public BackgroundTaskQueue(IConfiguration configuration)
        {
            _capacity = configuration.GetValue("BackgroundQueue:Capacity", 500);
            _queue = Channel.CreateBounded<Guid>(new BoundedChannelOptions(_capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
        }

        public int Capacity => _capacity;

        public int PendingCount => _queue.Reader.Count;

        public bool IsFull => _queue.Reader.Count >= _capacity;
        
        public ValueTask EnqueueAsync(Guid scanId, CancellationToken ct = default)
            => _queue.Writer.WriteAsync(scanId, ct);

        public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken ct = default)
            => _queue.Reader.ReadAllAsync(ct);
    }
}
