namespace DictionaryProvider.Api.Dtos.Dictionary;

public record TranslationDto
{
    public string? Language { get; init; }
    public string Text { get; init; } = string.Empty;
}
