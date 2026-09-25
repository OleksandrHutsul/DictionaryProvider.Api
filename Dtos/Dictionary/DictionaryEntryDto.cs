namespace DictionaryProvider.Api.Dtos.Dictionary;

public record DictionaryEntryDto
{
    public string? PartOfSpeech { get; init; }

    public IReadOnlyList<GuideWordGroupDto> GuideWordGroups { get; init; } = [];
}
