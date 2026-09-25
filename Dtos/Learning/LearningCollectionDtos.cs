namespace DictionaryProvider.Api.Dtos.Learning;

public class LearningCollectionDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Accent { get; init; }
    public bool IsDefault { get; init; }
    public IReadOnlyList<LearningCollectionWordDto> Words { get; init; } = [];
}
