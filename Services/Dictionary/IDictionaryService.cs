using DictionaryProvider.Api.Dtos.Dictionary;

namespace DictionaryProvider.Api.Services.Dictionary;

public interface IDictionaryService
{
    Task<DictionaryWordDto?> GetWordAsync(string word, CancellationToken cancellationToken);
    Task<IReadOnlyList<DictionarySuggestionDto>> SearchAsync(string query, CancellationToken cancellationToken);
    Task<DictionaryWordDto?> GetSuggestionAsync(DictionarySuggestionDto suggestion, CancellationToken cancellationToken);
}
