using System.Net;
using System.Text;
using System.Text.Json;

namespace DictionaryProvider.Api.Services.PhotoTranslation;

public partial class PhotoTranslationService
{
    private async Task<string> TranslateWithFallbackAsync(string text, CancellationToken cancellationToken)
    {
        try
        {
            return await TranslateWithGoogleAsync(text, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(exception, "Google translation failed; trying MyMemory. Text length: {Length}", text.Length);
            return await TranslateWithMyMemoryAsync(text, cancellationToken);
        }
    }

    private async Task<string> TranslateWithGoogleAsync(string text, CancellationToken cancellationToken)
    {
        var endpoint = _options.TranslationFallbackEndpoint.TrimEnd('?', '&');
        var url = $"{endpoint}?client=gtx&sl=en&tl=uk&dt=t&q={Uri.EscapeDataString(text)}";

        _logger.LogInformation("Google translation request. Text length: {Length}", text.Length);

        var client = _httpClientFactory.CreateClient(HttpClientName);

        using var response = await client.GetAsync(url, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Google translation returned {StatusCode} {Reason}. Body: {Body}", (int)response.StatusCode, response.ReasonPhrase, Truncate(body, 400));
            throw new InvalidOperationException($"Google translation returned {(int)response.StatusCode} {response.ReasonPhrase}.");
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
                throw new InvalidOperationException("Google translation returned an invalid response.");

            var segments = root[0];

            if (segments.ValueKind != JsonValueKind.Array)
                throw new InvalidOperationException("Google translation returned an invalid response.");

            var translated = string.Concat(segments.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.Array && item.GetArrayLength() > 0)
                .Select(item => item[0].GetString() ?? ""));

            translated = WebUtility.HtmlDecode(translated).Trim();

            if (string.IsNullOrWhiteSpace(translated))
                throw new InvalidOperationException("Google translation returned empty text.");

            _logger.LogInformation("Google translation succeeded. Output length: {Length}", translated.Length);

            return translated;
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "Google translation response could not be deserialized. Body: {Body}", Truncate(body, 400));
            throw new InvalidOperationException("Google translation response could not be deserialized.", exception);
        }
    }

    private async Task<string> TranslateWithMyMemoryAsync(string text, CancellationToken cancellationToken)
    {
        var query = new StringBuilder()
            .Append(_options.TranslationEndpoint.TrimEnd('?', '&'))
            .Append("?q=").Append(Uri.EscapeDataString(text))
            .Append("&langpair=").Append(Uri.EscapeDataString("en|uk"));

        if (!string.IsNullOrWhiteSpace(_options.TranslationEmail))
            query.Append("&de=").Append(Uri.EscapeDataString(_options.TranslationEmail.Trim()));

        var url = query.ToString();

        _logger.LogInformation("MyMemory request. Text length: {Length}. HasEmail: {HasEmail}. UrlHostPath: {Url}",
            text.Length, !string.IsNullOrWhiteSpace(_options.TranslationEmail), RedactQuery(url));

        var client = _httpClientFactory.CreateClient(HttpClientName);

        using var response = await client.GetAsync(url, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("MyMemory returned {StatusCode} {Reason}. Body: {Body}", (int)response.StatusCode, response.ReasonPhrase, Truncate(body, 400));
            throw new InvalidOperationException($"Translation provider returned {(int)response.StatusCode} {response.ReasonPhrase}.");
        }

        JsonElement json;

        try
        {
            json = JsonSerializer.Deserialize<JsonElement>(body);
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "MyMemory response could not be deserialized. Body: {Body}", Truncate(body, 400));
            throw new InvalidOperationException("Translation response could not be deserialized.", exception);
        }

        var status = json.TryGetProperty("responseStatus", out var statusElement) && statusElement.TryGetInt32(out var code)
            ? code
            : (int?)null;

        if (!json.TryGetProperty("responseData", out var data) || !data.TryGetProperty("translatedText", out var value))
        {
            _logger.LogError("MyMemory response missing translatedText. Body: {Body}", Truncate(body, 400));
            throw new InvalidOperationException("The translation provider returned an invalid response.");
        }

        var translated = WebUtility.HtmlDecode(value.GetString() ?? "").Trim();

        if (status is >= 400 || IsProviderQuotaMessage(translated))
        {
            var details = json.TryGetProperty("responseDetails", out var detailsElement)
                ? detailsElement.GetString()
                : translated;

            _logger.LogError("MyMemory quota/error. responseStatus: {Status}. Details: {Details}", status, Truncate(details, 300));

            if (status == 429 || IsProviderQuotaMessage(translated))
                throw new InvalidOperationException("Translation provider daily quota exceeded.");

            throw new InvalidOperationException($"Translation provider error (status {status}): {Truncate(details, 160)}");
        }

        return translated;
    }

    private static bool IsProviderQuotaMessage(string text)
    {
        return text.Contains("MYMEMORY WARNING", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("YOU USED ALL AVAILABLE FREE TRANSLATIONS", StringComparison.OrdinalIgnoreCase);
    }

    private static string RedactQuery(string url)
    {
        try
        {
            var uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
        }
        catch
        {
            return Truncate(url, 80);
        }
    }
}
