namespace DictionaryProvider.Api.Dtos.Feedback;

public class FeedbackRequest
{
    public string Type { get; set; } = "Bug";
    public string Description { get; set; } = "";
    public string? PageOrFeature { get; set; }
    public string? ContactEmail { get; set; }
    public string? Diagnostics { get; set; }
}
