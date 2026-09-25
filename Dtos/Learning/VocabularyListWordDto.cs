namespace DictionaryProvider.Api.Dtos.Learning;

public class VocabularyListWordDto
{
    public Guid Id { get; init; }
    public Guid? DictionaryWordId { get; init; }
    public required string Word { get; init; }
    public DateTimeOffset AddedAt { get; init; }
}
