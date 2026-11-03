namespace TestKaspersky.Models
{
    public class CancelScanResponse
    {
        public Guid ScanId { get; set; }
        public DateTime CancelledAt { get; set; }
        public bool Success { get; set; }
    }
}
