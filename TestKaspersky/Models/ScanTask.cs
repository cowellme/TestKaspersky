namespace TestKaspersky.Models
{
    public enum ScanStatus
    {
        Queued,
        InProgress,
        Completed,
        Failed,
        Cancelled
    }

    public class ScanTask
    {
        public Guid Id { get; set; }
        public string RepositoryUrl { get; set; } = string.Empty;
        public ScanStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public List<Guid> VulnerabilityIds { get; set; } = [];
    }
}
