namespace DictionaryProvider.Api.Entities;

public class VocabularyList
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }
    public ApplicationUser? Owner { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsArchived { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }
    public ICollection<VocabularyListWord> Words { get; set; } = [];
    public ICollection<VocabularyListShare> Shares { get; set; } = [];
    public ICollection<VocabularyListTestResult> TestResults { get; set; } = [];
}
