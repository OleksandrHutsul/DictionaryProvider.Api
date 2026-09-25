namespace DictionaryProvider.Api.Entities;

public class LearningCollectionProfile
{
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<LearningCollectionEntity> Collections { get; set; } = [];
}
