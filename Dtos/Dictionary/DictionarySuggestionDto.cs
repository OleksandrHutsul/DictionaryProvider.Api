namespace DictionaryProvider.Api.Dtos.Dictionary;

public record DictionarySuggestionDto
{
    public required string Word { get; init; }
    public string? Url { get; init; }
}
