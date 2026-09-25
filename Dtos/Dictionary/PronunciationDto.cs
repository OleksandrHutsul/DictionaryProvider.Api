namespace DictionaryProvider.Api.Dtos.Dictionary;

public record PronunciationDto
{
    public string Dialect { get; init; } = string.Empty;
    public string? Ipa { get; init; }
    public string? AudioUrl { get; init; }
}
