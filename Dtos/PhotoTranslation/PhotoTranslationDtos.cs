namespace DictionaryProvider.Api.Dtos.PhotoTranslation;

public class PhotoTranslationDto
{
    public required string OriginalText { get; init; }
    public required string TranslatedText { get; init; }
    public IReadOnlyList<PhotoWordTranslationDto> Words { get; init; } = [];

    public string? TranslationError { get; init; }
}
