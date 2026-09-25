namespace DictionaryProvider.Api.Entities;

public class VocabularyListTestResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VocabularyListId { get; set; }
    public VocabularyList? VocabularyList { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public DateTimeOffset CompletedAt { get; set; } = DateTimeOffset.UtcNow;
}
