using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DictionaryProvider.Api.Configuration;
using DictionaryProvider.Api.Entities;
using DictionaryProvider.Api.Services.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DictionaryProvider.Api.Services.Token;

public class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(ApplicationUser user, bool rememberMe, bool isAdmin = false)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(GetExpirationMinutes(rememberMe));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email)
        };

        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, AdminAuthorization.AdminRole));
            claims.Add(new Claim("role", AdminAuthorization.AdminRole));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private int GetExpirationMinutes(bool rememberMe)
    {
        return rememberMe ? _options.RememberMeDays * 24 * 60 : _options.AccessTokenMinutes;
    }
}
