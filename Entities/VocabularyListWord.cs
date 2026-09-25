namespace DictionaryProvider.Api.Entities;

public class VocabularyListWord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VocabularyListId { get; set; }
    public VocabularyList? VocabularyList { get; set; }
    public Guid? DictionaryWordId { get; set; }
    public DictionaryWord? DictionaryWord { get; set; }
    public required string DisplayWord { get; set; }
    public required string NormalizedWord { get; set; }
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
}
