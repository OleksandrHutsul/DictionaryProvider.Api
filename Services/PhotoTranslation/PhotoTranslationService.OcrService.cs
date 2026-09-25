using System.Text.Json;

namespace DictionaryProvider.Api.Services.PhotoTranslation;

public partial class PhotoTranslationService
{
    private async Task<string> RecognizeAsync(Stream image, string fileName, string contentType, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        using var file = new StreamContent(image);

        file.Headers.ContentType = new(contentType);

        content.Add(file, "file", fileName);
        content.Add(new StringContent(_options.OcrApiKey), "apikey");
        content.Add(new StringContent("eng"), "language");
        content.Add(new StringContent("true"), "scale");
        content.Add(new StringContent("2"), "OCREngine");

        _logger.LogInformation("Photo OCR request. Endpoint: {Endpoint}. File: {FileName}. ContentType: {ContentType}", _options.OcrEndpoint, fileName, contentType);

        var client = _httpClientFactory.CreateClient(HttpClientName);

        using var response = await client.PostAsync(_options.OcrEndpoint, content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OCR provider returned {StatusCode}. Body: {Body}", (int)response.StatusCode, Truncate(body, 500));
            throw new InvalidOperationException($"OCR provider returned {(int)response.StatusCode} {response.ReasonPhrase}.");
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        if (root.TryGetProperty("IsErroredOnProcessing", out var errored) && errored.GetBoolean())
        {
            var error = ReadOcrError(root);

            _logger.LogError("OCR provider reported a processing error: {Error}", error);
            throw new InvalidOperationException(error);
        }

        if (!root.TryGetProperty("ParsedResults", out var results) || results.ValueKind != JsonValueKind.Array)
            return "";

        return string.Join("\n\n", results.EnumerateArray()
            .Select(item => item.TryGetProperty("ParsedText", out var value) ? value.GetString()?.Trim() : null)
            .Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private static string ReadOcrError(JsonElement root)
    {
        if (!root.TryGetProperty("ErrorMessage", out var message))
            return "No readable text was detected in the image.";

        if (message.ValueKind == JsonValueKind.Array)
            return string.Join(" ", message.EnumerateArray().Select(item => item.GetString()));

        return message.ToString();
    }
}
