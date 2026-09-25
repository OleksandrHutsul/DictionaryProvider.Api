namespace DictionaryProvider.Api.Entities;

public class DictionaryWord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Word { get; set; }
    public required string NormalizedWord { get; set; }
    public string? Provider { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
