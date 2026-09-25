namespace DictionaryProvider.Api.Dtos.Feedback;

public class FeedbackReportDto
{
    public Guid Id { get; init; }
    public required string Type { get; init; }
    public required string Description { get; init; }
    public string? PageOrFeature { get; init; }
    public string? ContactEmail { get; init; }
    public string? Diagnostics { get; init; }
    public string Status { get; init; } = "New";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public Guid? UserId { get; init; }
    public string? UserEmail { get; init; }
}
