namespace DictionaryProvider.Api.Dtos.Learning;

public class CreateVocabularyListRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
