namespace DictionaryProvider.Api.Dtos.PhotoTranslation;

public class PhotoWordTranslationDto
{
    public required string Word { get; init; }
    public string? Translation { get; init; }
    public string? Error { get; init; }
}
