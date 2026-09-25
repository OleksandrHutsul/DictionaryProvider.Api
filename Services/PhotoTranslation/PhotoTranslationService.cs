using System.Text;
using System.Text.RegularExpressions;
using DictionaryProvider.Api.Configuration;
using DictionaryProvider.Api.Dtos.PhotoTranslation;
using DictionaryProvider.Api.Enums;
using DictionaryProvider.Api.Services.Dictionary;
using Microsoft.Extensions.Options;

namespace DictionaryProvider.Api.Services.PhotoTranslation;

public partial class PhotoTranslationService : IPhotoTranslationService
{
    private const string HttpClientName = "PhotoTranslation";
    private const string UkrainianUnavailable = "Ukrainian translation unavailable.";
    private const int TranslationChunkSize = 1500;
    private const int WordBatchSize = 400;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PhotoTranslationOptions _options;
    private readonly IDictionaryService _dictionaryService;
    private readonly ILogger<PhotoTranslationService> _logger;

    public PhotoTranslationService(IHttpClientFactory httpClientFactory, IOptions<PhotoTranslationOptions> options, IDictionaryService dictionaryService, ILogger<PhotoTranslationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _dictionaryService = dictionaryService;
        _logger = logger;
    }

    public async Task<PhotoTranslationDto> TranslateAsync(Stream image, string fileName, string contentType, PhotoTranslationMode mode, CancellationToken cancellationToken)
    {
        var text = await RecognizeAsync(image, fileName, contentType, cancellationToken);

        _logger.LogInformation("Photo OCR succeeded. Recognized text length: {Length}", text.Length);

        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("Photo OCR returned empty text.");
            return new PhotoTranslationDto { OriginalText = "", TranslatedText = "" };
        }

        return await TranslateRecognizedTextAsync(text, mode, cancellationToken);
    }

    public async Task<PhotoTranslationDto> TranslateRecognizedTextAsync(string originalText, PhotoTranslationMode mode, CancellationToken cancellationToken)
    {
        var text = originalText?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(text))
            return new PhotoTranslationDto { OriginalText = "", TranslatedText = "" };

        _logger.LogInformation("Photo translation starting. Mode: {Mode}. Recognized text length: {Length}. Preview: {Preview}", mode, text.Length, Truncate(text, 120));

        try
        {
            if (mode == PhotoTranslationMode.Text)
            {
                return new PhotoTranslationDto
                {
                    OriginalText = text,
                    TranslatedText = await TranslateTextAsync(text, cancellationToken)
                };
            }

            if (mode == PhotoTranslationMode.WordList)
            {
                return new PhotoTranslationDto
                {
                    OriginalText = text,
                    TranslatedText = "",
                    Words = await TranslateWordsAsync(ExtractWords(text), cancellationToken)
                };
            }

            throw new InvalidOperationException("Unsupported photo translation mode.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Photo translation failed after OCR. Mode: {Mode}. Recognized text length: {Length}. Reason: {Reason}", mode, text.Length, exception.Message);

            return new PhotoTranslationDto
            {
                OriginalText = text,
                TranslatedText = "",
                TranslationError = ToUserFacingTranslationError(exception),
                Words = mode == PhotoTranslationMode.WordList
                    ? ExtractWords(text).Select(word => new PhotoWordTranslationDto { Word = word, Error = UkrainianUnavailable }).ToArray()
                    : []
            };
        }
    }

    private async Task<string> TranslateTextAsync(string text, CancellationToken cancellationToken)
    {
        var paragraphs = GetTranslationParagraphs(text);
        var translatedParagraphs = new List<string>(paragraphs.Count);

        foreach (var paragraph in paragraphs)
        {
            var chunks = CreateTranslationChunks(paragraph);
            var translatedChunks = await Task.WhenAll(chunks.Select(chunk => TranslateWithFallbackAsync(chunk, cancellationToken)));
            translatedParagraphs.Add(string.Join(" ", translatedChunks));
        }

        var result = string.Join("\n\n", translatedParagraphs);

        if (!IsUkrainianTranslation(text, result))
        {
            _logger.LogError("Translation result failed Ukrainian validation. Source length: {SourceLength}. Result preview: {Preview}", text.Length, Truncate(result, 200));
            throw new InvalidOperationException("The translation provider did not return a Ukrainian translation.");
        }

        _logger.LogInformation("Photo text translation succeeded. Output length: {Length}", result.Length);

        return result;
    }

    private async Task<IReadOnlyList<PhotoWordTranslationDto>> TranslateWordsAsync(IReadOnlyList<string> words, CancellationToken cancellationToken)
    {
        if (words.Count == 0) return [];

        var candidates = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var batch in Batch(words, WordBatchSize))
        {
            var translations = await TranslateWordBatchAsync(batch, cancellationToken);

            foreach (var item in translations)
                candidates[item.Key] = item.Value;
        }

        var results = new List<PhotoWordTranslationDto>(words.Count);

        foreach (var word in words)
        {
            candidates.TryGetValue(word, out var translation);

            if (!IsUkrainianTranslation(word, translation))
                translation = await GetDictionaryTranslationAsync(word, cancellationToken);

            results.Add(IsUkrainianTranslation(word, translation)
                ? new PhotoWordTranslationDto { Word = word, Translation = translation!.Trim() }
                : new PhotoWordTranslationDto { Word = word, Error = UkrainianUnavailable });
        }

        return results;
    }

    private async Task<IReadOnlyDictionary<string, string?>> TranslateWordBatchAsync(IReadOnlyList<string> words, CancellationToken cancellationToken)
    {
        try
        {
            var translated = await TranslateWithFallbackAsync(string.Join('\n', words), cancellationToken);
            var lines = Regex.Split(translated, "\\r?\\n")
                .Select(value => value.Trim())
                .Where(value => value.Length > 0)
                .ToList();

            if (lines.Count != words.Count)
                return words.ToDictionary(word => word, _ => (string?)null, StringComparer.OrdinalIgnoreCase);

            return words.Select((word, index) => new KeyValuePair<string, string?>(word, lines[index]))
                .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(exception, "Word-batch translation failed; falling back to dictionary lookups.");
            return words.ToDictionary(word => word, _ => (string?)null, StringComparer.OrdinalIgnoreCase);
        }
    }

    private async Task<string?> GetDictionaryTranslationAsync(string word, CancellationToken cancellationToken)
    {
        try
        {
            var entry = await _dictionaryService.GetWordAsync(word, cancellationToken);
            return entry?.Translations.Select(item => item.Text).FirstOrDefault(value => IsUkrainianTranslation(word, value));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogDebug(exception, "Dictionary fallback failed for word {Word}", word);
            return null;
        }
    }

    private static IReadOnlyList<string> GetTranslationParagraphs(string text)
    {
        return Regex.Split(text.Trim(), @"(?:\r?\n\s*){2,}")
            .Select(NormalizeParagraph)
            .Where(paragraph => paragraph.Length > 0)
            .ToList();
    }

    private static string NormalizeParagraph(string paragraph)
    {
        return Regex.Replace(paragraph, @"\s*\r?\n\s*", " ").Trim();
    }

    private static IReadOnlyList<string> CreateTranslationChunks(string paragraph)
    {
        var sentences = SplitSentences(paragraph);

        if (sentences.Count == 0) return [paragraph];

        var chunks = new List<string>();
        var current = new StringBuilder();

        foreach (var sentence in sentences)
        {
            if (current.Length > 0 && current.Length + sentence.Length + 1 > TranslationChunkSize)
            {
                chunks.Add(current.ToString());
                current.Clear();
            }

            if (current.Length > 0) current.Append(' ');

            current.Append(sentence);
        }

        if (current.Length > 0) chunks.Add(current.ToString());

        return chunks;
    }

    private static IReadOnlyList<string> SplitSentences(string text)
    {
        return Regex.Matches(text, @".+?(?:[.!?]+(?=\s|$)|$)")
            .Select(match => match.Value.Trim())
            .Where(sentence => sentence.Length > 0)
            .ToList();
    }

    private static IReadOnlyList<string> ExtractWords(string text)
    {
        return Regex.Matches(text, @"[A-Za-z]+(?:['’\-][A-Za-z]+)*")
            .Select(match => match.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<IReadOnlyList<string>> Batch(IReadOnlyList<string> words, int maxCharacters)
    {
        var batches = new List<IReadOnlyList<string>>();
        var current = new List<string>();
        var length = 0;

        foreach (var word in words)
        {
            if (current.Count > 0 && length + word.Length + 1 > maxCharacters)
            {
                batches.Add(current);
                current = [];
                length = 0;
            }

            current.Add(word);
            length += word.Length + 1;
        }

        if (current.Count > 0) batches.Add(current);

        return batches;
    }

    private static bool IsUkrainianTranslation(string source, string? translation)
    {
        return !string.IsNullOrWhiteSpace(translation) &&
               !source.Trim().Equals(translation.Trim(), StringComparison.OrdinalIgnoreCase) &&
               !IsProviderQuotaMessage(translation) &&
               Regex.IsMatch(translation, "[А-ЩЬЮЯЄІЇҐа-щьюяєіїґ]");
    }

    private static string ToUserFacingTranslationError(Exception exception)
    {
        return exception.Message.Contains("quota", StringComparison.OrdinalIgnoreCase)
            ? "Translation quota was exceeded. Recognized text was kept — you can retry translation."
            : "Translation failed after text recognition. Recognized text was kept — you can retry translation.";
    }

    private static string Truncate(string? value, int max)
    {
        return string.IsNullOrEmpty(value) ? "" : value.Length <= max ? value : value[..max] + "…";
    }
}
