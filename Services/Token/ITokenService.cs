using DictionaryProvider.Api.Entities;

namespace DictionaryProvider.Api.Services.Token;

public interface ITokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(ApplicationUser user, bool rememberMe, bool isAdmin = false);
}