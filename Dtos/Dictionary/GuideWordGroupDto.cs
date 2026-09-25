namespace DictionaryProvider.Api.Dtos.Dictionary;

public record GuideWordGroupDto
{
    public string? GuideWord { get; init; }

    public IReadOnlyList<MeaningDto> Meanings { get; init; } = [];
}
