namespace DictionaryProvider.Api.Configuration;

public class CambridgeOptions
{
    public const string SectionName = "Cambridge";

    public string BaseUrl { get; set; } = string.Empty;
    public string TranslationBaseUrl { get; set; } = string.Empty;
    public string AutocompleteUrl { get; set; } = string.Empty;
    public string AutocompleteDataset { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; }
    public string DictionaryVariant { get; set; } = string.Empty;
    public string TranslationLanguage { get; set; } = string.Empty;
    public string SiteUrl { get; set; } = string.Empty;
}
