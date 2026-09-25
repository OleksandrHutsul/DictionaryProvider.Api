using DictionaryProvider.Api.Configuration;
using DictionaryProvider.Api.Data;
using DictionaryProvider.Api.Dtos.Authentication;
using DictionaryProvider.Api.Entities;
using DictionaryProvider.Api.Services.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DictionaryProvider.Api.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly AdminOptions _adminOptions;

    public AuthenticationService(ApplicationDbContext dbContext, IPasswordHasher<ApplicationUser> passwordHasher, ITokenService tokenService, IOptions<AdminOptions> adminOptions)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _adminOptions = adminOptions.Value;
    }

    public async Task<AuthenticationResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = Normalize(request.Email);
        var normalizedUserName = Normalize(request.UserName);

        var emailExists = await _dbContext.Users
            .AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

        if (emailExists)
            throw new AuthenticationValidationException("Email", "An account with this email already exists.");

        var user = new ApplicationUser
        {
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            UserName = request.UserName.Trim(),
            NormalizedUserName = normalizedUserName,
            PasswordHash = ""
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreateResponse(user, rememberMe: true);
    }

    public async Task<AuthenticationResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = Normalize(request.Email);

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user is null)
            throw new AuthenticationValidationException("Email", "Invalid email or password.");

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (verification == PasswordVerificationResult.Failed)
            throw new AuthenticationValidationException("Email", "Invalid email or password.");

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        user.LastLoginAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreateResponse(user, request.RememberMe);
    }

    public async Task<AuthenticatedUserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

        return user is null ? null : MapUser(user);
    }

    private AuthenticationResponse CreateResponse(ApplicationUser user, bool rememberMe)
    {
        var isAdmin = AdminAuthorization.IsDeveloperEmail(user.Email, _adminOptions);
        var token = _tokenService.CreateAccessToken(user, rememberMe, isAdmin);

        return new AuthenticationResponse
        {
            AccessToken = token.Token,
            ExpiresAt = token.ExpiresAt,
            User = MapUser(user, isAdmin)
        };
    }

    private AuthenticatedUserDto MapUser(ApplicationUser user, bool? isAdmin = null)
    {
        return new AuthenticatedUserDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            CreatedAt = user.CreatedAt,
            IsAdmin = isAdmin ?? AdminAuthorization.IsDeveloperEmail(user.Email, _adminOptions)
        };
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }
}
