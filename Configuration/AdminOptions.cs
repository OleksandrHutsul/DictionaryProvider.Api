namespace DictionaryProvider.Api.Configuration;

public class AdminOptions
{
    public const string SectionName = "Admin";

    public string[] DeveloperEmails { get; set; } = [];
}
