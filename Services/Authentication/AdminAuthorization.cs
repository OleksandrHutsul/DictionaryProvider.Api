using DictionaryProvider.Api.Configuration;

namespace DictionaryProvider.Api.Services.Authentication;

public static class AdminAuthorization
{
    public const string DeveloperPolicy = "Developer";
    public const string AdminRole = "Admin";

    public static bool IsDeveloperEmail(string? email, AdminOptions options)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        var normalizedEmail = email.Trim();

        return options.DeveloperEmails.Any(developerEmail => developerEmail.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));
    }
}