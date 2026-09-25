namespace DictionaryProvider.Api.Dtos.Notifications;

public class NotificationDto
{
    public Guid Id { get; init; }
    public required string Type { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public Guid? RelatedListId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public bool IsRead { get; init; }
}
