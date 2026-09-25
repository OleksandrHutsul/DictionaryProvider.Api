namespace DictionaryProvider.Api.Dtos.Dictionary;

public record RelatedTermDto
{
    public string Text { get; init; } = string.Empty;
    public string? Url { get; init; }
}
