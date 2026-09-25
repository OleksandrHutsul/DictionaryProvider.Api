using DictionaryProvider.Api.Enums;

namespace DictionaryProvider.Api.Entities;

public class VocabularyListShare
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VocabularyListId { get; set; }
    public VocabularyList? VocabularyList { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public VocabularyListPermission Permission { get; set; } = VocabularyListPermission.Reader;
    public DateTimeOffset SharedAt { get; set; } = DateTimeOffset.UtcNow;
}
