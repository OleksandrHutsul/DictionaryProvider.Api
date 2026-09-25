namespace DictionaryProvider.Api.Dtos.Dictionary;

public record DictionaryWordDto
{
    public string Word { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string Origin { get; init; } = string.Empty;

    public IReadOnlyList<PronunciationDto> Pronunciations { get; init; } = [];
    public IReadOnlyList<DictionaryEntryDto> Meanings { get; init; } = [];
    public IReadOnlyList<TranslationDto> Translations { get; init; } = [];
    public IReadOnlyList<RelatedTermDto> Synonyms { get; init; } = [];
    public IReadOnlyList<RelatedTermDto> Antonyms { get; init; } = [];
    public IReadOnlyList<RelatedTermDto> PhrasalVerbs { get; init; } = [];
    public IReadOnlyList<RelatedTermDto> Idioms { get; init; } = [];
    public IReadOnlyList<CollocationDto> Collocations { get; init; } = [];
}
