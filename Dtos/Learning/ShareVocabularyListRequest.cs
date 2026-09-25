namespace DictionaryProvider.Api.Dtos.Learning;

public class ShareVocabularyListRequest
{
    public Guid? UserId { get; init; }
    public string? Email { get; init; }
    public string Permission { get; init; } = "Reader";
}
