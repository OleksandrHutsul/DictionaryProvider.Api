using System.Text.RegularExpressions;
using DictionaryProvider.Api.Dtos.Dictionary;
using DictionaryProvider.Api.Services.Dictionary;
using DictionaryProvider.Api.Services.DictionaryQuery;

namespace DictionaryProvider.Api.Services.DictionaryLookup;

public class DictionaryLookupService : IDictionaryLookupService
{
    private readonly IDictionaryService _dictionaryService;
    private readonly IDictionaryQueryService _queryService;

    public DictionaryLookupService(IDictionaryService dictionaryService, IDictionaryQueryService queryService)
    {
        _dictionaryService = dictionaryService;
        _queryService = queryService;
    }

    public async Task<DictionaryLookupResultDto> LookupAsync(string query, CancellationToken cancellationToken = default)
    {
        var phrase = _queryService.Normalize(query);
        if (phrase.Length == 0) return new DictionaryLookupResultDto(phrase, phrase, null, [], false);

        var candidates = _queryService.GetCandidates(phrase);

        foreach (var candidate in candidates)
        {
            var (entry, suggestions) = await LookupCandidateAsync(candidate, cancellationToken);

            if (entry is not null || suggestions.Count > 0)
                return new DictionaryLookupResultDto(phrase, candidate, entry, suggestions, true);
        }

        return new DictionaryLookupResultDto(phrase, candidates.FirstOrDefault() ?? phrase, null, [], false);
    }

    private async Task<(DictionaryWordDto? Entry, IReadOnlyList<DictionarySuggestionDto> Suggestions)> LookupCandidateAsync(string candidate, CancellationToken cancellationToken)
    {
        var entry = await _dictionaryService.GetWordAsync(candidate, cancellationToken);

        if (entry is not null && IsExactMatch(candidate, entry.Word))
            return (entry, []);

        if (!IsMultiWord(candidate))
            return (null, []);

        var suggestions = await _dictionaryService.SearchAsync(candidate, cancellationToken);
        var matches = suggestions
            .Where(suggestion => IsPhraseMatch(candidate, suggestion.Word))
            .ToList();

        return (null, matches);
    }

    private bool IsExactMatch(string candidate, string word)
    {
        var normalizedCandidate = _queryService.Normalize(candidate);
        var normalizedWord = _queryService.Normalize(word);

        return normalizedCandidate.Equals(normalizedWord, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsPhraseMatch(string candidate, string suggestion)
    {
        var normalizedCandidate = NormalizePhrase(candidate);
        var normalizedSuggestion = NormalizePhrase(suggestion);

        return normalizedSuggestion.Equals(normalizedCandidate, StringComparison.OrdinalIgnoreCase) ||
               normalizedSuggestion.StartsWith(normalizedCandidate + " ", StringComparison.OrdinalIgnoreCase);
    }

    private string NormalizePhrase(string value)
    {
        var withoutParentheses = Regex.Replace(value ?? "", @"\([^)]*\)", " ");
        return _queryService.Normalize(withoutParentheses);
    }

    private static bool IsMultiWord(string value)
    {
        return value.Contains(' ');
    }
}
