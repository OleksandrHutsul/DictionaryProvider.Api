using DictionaryProvider.Api.Dtos.Dictionary;
using DictionaryProvider.Api.Services.DictionaryQuery;
using DictionaryProvider.Api.Services.Providers;

namespace DictionaryProvider.Api.Services.Dictionary;

public class DictionaryService : IDictionaryService
{
    private readonly ICambridgeProvider _cambridgeProvider;
    private readonly IDictionaryQueryService _queryService;

    public DictionaryService(ICambridgeProvider cambridgeProvider, IDictionaryQueryService queryService)
    {
        _cambridgeProvider = cambridgeProvider;
        _queryService = queryService;
    }

    public async Task<DictionaryWordDto?> GetWordAsync(string word, CancellationToken cancellationToken)
    {
        var normalizedWord = word.Trim();
        if (normalizedWord.Length == 0) return null;

        return await _cambridgeProvider.GetWordAsync(normalizedWord, cancellationToken);
    }

    public async Task<IReadOnlyList<DictionarySuggestionDto>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var normalizedQuery = query.Trim();
        if (normalizedQuery.Length < 2) return [];

        var candidates = _queryService.GetCandidates(normalizedQuery, baseFormFirst: false);

        foreach (var candidate in candidates)
        {
            var suggestions = await _cambridgeProvider.SearchAsync(candidate, cancellationToken);
            if (suggestions.Count > 0) return suggestions;
        }

        return [];
    }

    public async Task<DictionaryWordDto?> GetSuggestionAsync(DictionarySuggestionDto suggestion, CancellationToken cancellationToken)
    {
        return await _cambridgeProvider.GetSuggestionAsync(suggestion, cancellationToken);
    }
}
