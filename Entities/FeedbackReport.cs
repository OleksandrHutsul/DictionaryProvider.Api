using DictionaryProvider.Api.Enums;

namespace DictionaryProvider.Api.Entities;

public class FeedbackReport
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Type { get; set; }
    public required string Description { get; set; }
    public string? PageOrFeature { get; set; }
    public string? ContactEmail { get; set; }
    public string? Diagnostics { get; set; }
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public FeedbackReportStatus Status { get; set; } = FeedbackReportStatus.New;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
