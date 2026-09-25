namespace DictionaryProvider.Api.Dtos.Authentication;

public class AuthenticationResponse
{
    public required string AccessToken { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required AuthenticatedUserDto User { get; init; }
}
