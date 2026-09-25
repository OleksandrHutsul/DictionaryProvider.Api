namespace DictionaryProvider.Api.Dtos.Learning;

public class VocabularyListShareDto
{
    public Guid UserId { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string Permission { get; init; }
    public DateTimeOffset SharedAt { get; init; }
}
