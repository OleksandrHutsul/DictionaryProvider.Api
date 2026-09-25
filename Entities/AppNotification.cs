namespace DictionaryProvider.Api.Entities;

public class AppNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RecipientUserId { get; set; }
    public ApplicationUser? RecipientUser { get; set; }
    public string Type { get; set; } = "VocabularyListShared";
    public required string Title { get; set; }
    public required string Message { get; set; }
    public Guid? RelatedListId { get; set; }
    public VocabularyList? RelatedList { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
