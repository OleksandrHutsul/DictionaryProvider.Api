using System.Net;
using System.Text.Json;
using DictionaryProvider.Api.Configuration;
using DictionaryProvider.Api.Dtos.Dictionary;
using DictionaryProvider.Api.Parsers.Cambridge;
using Microsoft.Extensions.Options;

namespace DictionaryProvider.Api.Services.Providers;

public class CambridgeProvider : ICambridgeProvider
{
    public const string HttpClientName = "Cambridge";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CambridgeOptions _options;
    private readonly WordParser _wordParser;
    private readonly TranslationParser _translationParser;

    public CambridgeProvider(IHttpClientFactory httpClientFactory, IOptions<CambridgeOptions> options, WordParser wordParser, TranslationParser translationParser)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _wordParser = wordParser;
        _translationParser = translationParser;
    }

    public async Task<DictionaryWordDto?> GetWordAsync(string word, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);

        using var response = await client.GetAsync(BuildWordUri(_options.BaseUrl, word), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();

        if (IsRedirectedToDictionaryRoot(response.RequestMessage?.RequestUri)) return null;

        var html = await response.Content.ReadAsStringAsync(cancellationToken);
        var parsedWord = _wordParser.Parse(html, _options.DictionaryVariant, word);

        if (parsedWord is null) return null;

        var translations = await GetTranslationsAsync(client, word, cancellationToken);

        return parsedWord with { Translations = translations };
    }

    public async Task<DictionaryWordDto?> GetSuggestionAsync(DictionarySuggestionDto suggestion, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(suggestion.Url)) return null;

        var client = _httpClientFactory.CreateClient(HttpClientName);

        using var response = await client.GetAsync(BuildSuggestionUri(suggestion.Url), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();

        if (IsRedirectedToDictionaryRoot(response.RequestMessage?.RequestUri)) return null;

        var html = await response.Content.ReadAsStringAsync(cancellationToken);
        var parsedWord = _wordParser.Parse(html, _options.DictionaryVariant, suggestion.Word);

        if (parsedWord is null) return null;

        var translations = await GetTranslationsAsync(client, parsedWord.Word, cancellationToken);

        return parsedWord with { Translations = translations };
    }

    public async Task<IReadOnlyList<DictionarySuggestionDto>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var normalizedQuery = query.Trim();
        if (normalizedQuery.Length < 2) return [];

        var client = _httpClientFactory.CreateClient(HttpClientName);

        using var request = new HttpRequestMessage(HttpMethod.Get, BuildAutocompleteUri(normalizedQuery));
        request.Headers.Accept.ParseAdd("application/json");

        using var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound) return [];

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var items = await JsonSerializer.DeserializeAsync<List<CambridgeAutocompleteItem>>(stream, JsonOptions, cancellationToken);

        if (items is null || items.Count == 0) return [];

        return items
            .Where(item => !string.IsNullOrWhiteSpace(item.Word))
            .GroupBy(item => item.Word!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Take(12)
            .Select(item => new DictionarySuggestionDto
            {
                Word = item.Word!.Trim(),
                Url = item.Url
            })
            .ToArray();
    }

    private async Task<IReadOnlyList<TranslationDto>> GetTranslationsAsync(HttpClient client, string word, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(BuildWordUri(_options.TranslationBaseUrl, word), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound) return [];

        response.EnsureSuccessStatusCode();

        if (IsRedirectedToDictionaryRoot(response.RequestMessage?.RequestUri, _options.TranslationBaseUrl)) return [];

        var html = await response.Content.ReadAsStringAsync(cancellationToken);

        return _translationParser.ParseDocument(html, _options.TranslationLanguage).ToArray();
    }

    private Uri BuildAutocompleteUri(string query)
    {
        var url = $"{_options.AutocompleteUrl.TrimEnd('/')}?dataset={Uri.EscapeDataString(_options.AutocompleteDataset)}&q={Uri.EscapeDataString(query)}";
        return new Uri(url);
    }

    private static Uri BuildSuggestionUri(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri)) return absoluteUri;

        return new Uri(new Uri("https://dictionary.cambridge.org"), url);
    }

    private static Uri BuildWordUri(string baseUrl, string word)
    {
        var root = baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/";
        var slug = string.Join('-', word.Trim().ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        return new Uri(new Uri(root), Uri.EscapeDataString(slug));
    }

    private bool IsRedirectedToDictionaryRoot(Uri? requestUri, string? baseUrl = null)
    {
        if (requestUri is null) return false;

        var dictionaryUrl = baseUrl ?? _options.BaseUrl;
        return requestUri.AbsoluteUri.TrimEnd('/').Equals(dictionaryUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
    }
}
