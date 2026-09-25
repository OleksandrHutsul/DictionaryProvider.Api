namespace DictionaryProvider.Api.Dtos.Dictionary;

public record DictionaryLookupResultDto(string Query, string ResolvedQuery, DictionaryWordDto? Entry, IReadOnlyList<DictionarySuggestionDto> Suggestions, 
    bool IsFound);
