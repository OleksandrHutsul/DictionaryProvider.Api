using DictionaryProvider.Api.Entities;

namespace DictionaryProvider.Api.Services.FeedbackNotifier;

public interface IFeedbackNotifierService
{
    Task NotifyNewReportAsync(FeedbackReport report, CancellationToken cancellationToken = default);
}
