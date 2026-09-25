namespace DictionaryProvider.Api.Dtos.Learning;

public class VocabularyListSummaryDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public Guid OwnerId { get; init; }
    public required string OwnerName { get; init; }
    public int WordCount { get; init; }
    public bool IsOwner { get; init; }
    public required string Permission { get; init; }
    public bool IsArchived { get; init; }
    public DateTimeOffset? ArchivedAt { get; init; }
}
