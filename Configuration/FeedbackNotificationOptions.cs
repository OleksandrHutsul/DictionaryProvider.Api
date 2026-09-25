namespace DictionaryProvider.Api.Configuration;

public class FeedbackNotificationOptions
{
    public const string SectionName = "FeedbackNotification";

    public bool Enabled { get; set; }
    public string NotifyEmail { get; set; } = string.Empty;
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? SmtpFrom { get; set; }
    public bool UseSsl { get; set; }
}