using DictionaryProvider.Api.Dtos.Dictionary;

namespace DictionaryProvider.Api.Services.Providers;

public interface ICambridgeProvider
{
    Task<DictionaryWordDto?> GetWordAsync(string word, CancellationToken cancellationToken);
    Task<DictionaryWordDto?> GetSuggestionAsync(DictionarySuggestionDto suggestion, CancellationToken cancellationToken);
    Task<IReadOnlyList<DictionarySuggestionDto>> SearchAsync(string query, CancellationToken cancellationToken);
}
