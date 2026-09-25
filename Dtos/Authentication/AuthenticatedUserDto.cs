namespace DictionaryProvider.Api.Dtos.Authentication;

public class AuthenticatedUserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string UserName { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public bool IsAdmin { get; init; }
}
