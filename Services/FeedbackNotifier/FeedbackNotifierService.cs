using System.Net;
using System.Net.Mail;
using DictionaryProvider.Api.Configuration;
using DictionaryProvider.Api.Entities;
using Microsoft.Extensions.Options;

namespace DictionaryProvider.Api.Services.FeedbackNotifier;

public class FeedbackNotifierService : IFeedbackNotifierService
{
    private readonly FeedbackNotificationOptions _options;
    private readonly ILogger<FeedbackNotifierService> _logger;

    public FeedbackNotifierService(IOptions<FeedbackNotificationOptions> options, ILogger<FeedbackNotifierService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task NotifyNewReportAsync(FeedbackReport report, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.NotifyEmail))
        {
            _logger.LogInformation("Feedback notification skipped (disabled). Report {Id}", report.Id);
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.SmtpHost))
        {
            _logger.LogWarning("Feedback notification email not sent (SMTP host missing). Report {Id}. Notify: {Email}", report.Id, _options.NotifyEmail);

            return;
        }

        try
        {
            using var client = CreateSmtpClient();
            using var message = CreateMessage(report);

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("Feedback notification email sent for report {Id}", report.Id);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Failed to send feedback notification email for report {Id}", report.Id);
        }
    }

    private SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.UseSsl
        };

        if (!string.IsNullOrWhiteSpace(_options.SmtpUsername))
            client.Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword);

        return client;
    }

    private MailMessage CreateMessage(FeedbackReport report)
    {
        var from = _options.SmtpFrom ?? _options.SmtpUsername ?? "noreply@lexiflow.local";
        var subject = $"LexiFlow feedback: {report.Type}";

        return new MailMessage(from, _options.NotifyEmail, subject, BuildMessageBody(report));
    }

    private static string BuildMessageBody(FeedbackReport report)
    {
        return $"""
                New feedback report received.

                Id: {report.Id}
                Type: {report.Type}
                Status: {report.Status}
                Page: {report.PageOrFeature ?? "n/a"}
                Contact email: {report.ContactEmail ?? "not provided"}
                Created: {report.CreatedAt:u}

                Description:
                {report.Description}

                Diagnostics:
                {report.Diagnostics ?? "none"}
                """;
    }
}
