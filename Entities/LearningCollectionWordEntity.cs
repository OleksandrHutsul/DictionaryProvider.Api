namespace DictionaryProvider.Api.Entities;

public class LearningCollectionWordEntity
{
    public Guid Id { get; set; }
    public Guid CollectionId { get; set; }
    public LearningCollectionEntity? Collection { get; set; }
    public required string Word { get; set; }
    public required string NormalizedWord { get; set; }
    public int State { get; set; }
    public string? Level { get; set; }
    public string? Definition { get; set; }
    public string? Translation { get; set; }
    public DateTimeOffset AddedAt { get; set; }
    public DateTimeOffset? LastReviewedAt { get; set; }
    public int EasyCount { get; set; }
    public int HardCount { get; set; }
    public int AgainCount { get; set; }
}
