namespace DictionaryProvider.Api.Configuration;

public class PhotoTranslationOptions
{
    public const string SectionName = "PhotoTranslation";

    public string OcrEndpoint { get; set; } = string.Empty;
    public string OcrApiKey { get; set; } = string.Empty;
    public string TranslationEndpoint { get; set; } = string.Empty;
    public string? TranslationEmail { get; set; }
    public string TranslationFallbackEndpoint { get; set; } = string.Empty;
    public long MaxImageBytes { get; set; }
}