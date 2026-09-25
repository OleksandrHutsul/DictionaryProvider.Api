namespace DictionaryProvider.Api.Dtos.Learning;

public class UpdateVocabularyListRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
