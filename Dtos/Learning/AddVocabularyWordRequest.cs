namespace DictionaryProvider.Api.Dtos.Learning;

public class AddVocabularyWordRequest
{
    public Guid? DictionaryWordId { get; init; }
    public required string Word { get; init; }
}
