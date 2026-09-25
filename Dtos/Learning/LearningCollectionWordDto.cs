namespace DictionaryProvider.Api.Dtos.Learning;

public class LearningCollectionWordDto
{
    public Guid Id { get; init; }
    public required string Word { get; init; }
    public required string CollectionName { get; init; }
    public int State { get; init; }
    public string? Level { get; init; }
    public string? Definition { get; init; }
    public string? Translation { get; init; }
    public DateTimeOffset AddedAt { get; init; }
    public DateTimeOffset? LastReviewedAt { get; init; }
    public int EasyCount { get; init; }
    public int HardCount { get; init; }
    public int AgainCount { get; init; }
}
