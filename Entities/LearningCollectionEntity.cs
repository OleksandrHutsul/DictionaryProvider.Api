namespace DictionaryProvider.Api.Entities;

public class LearningCollectionEntity
{
    public Guid Id { get; set; }
    public Guid ProfileUserId { get; set; }
    public LearningCollectionProfile? Profile { get; set; }
    public required string Name { get; set; }
    public required string Accent { get; set; }
    public bool IsDefault { get; set; }
    public ICollection<LearningCollectionWordEntity> Words { get; set; } = [];
}
