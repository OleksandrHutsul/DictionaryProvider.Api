namespace DictionaryProvider.Api.Dtos.Dictionary;

public record MeaningDto
{
    public string? Definition { get; init; }
    public string? CefrLevel { get; init; }

    public IReadOnlyList<ExampleDto> Examples { get; init; } = [];
    public IReadOnlyList<TranslationDto> Translations { get; init; } = [];
    public IReadOnlyList<RelatedTermDto> Synonyms { get; init; } = [];
    public IReadOnlyList<RelatedTermDto> Antonyms { get; init; } = [];
}
