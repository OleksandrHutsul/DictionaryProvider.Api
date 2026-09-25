using DictionaryProvider.Api.Dtos.Dictionary;
using HtmlAgilityPack;

namespace DictionaryProvider.Api.Parsers.Cambridge;

public class TranslationParser
{
    private readonly HtmlTextNormalizer _normalizer;

    public TranslationParser(HtmlTextNormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    public IEnumerable<TranslationDto> Parse(HtmlNode node)
    {
        return ParseTranslationNodes(node, null);
    }

    public IEnumerable<TranslationDto> ParseDocument(string html, string language)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        return ParseTranslationNodes(document.DocumentNode, language);
    }

    private IEnumerable<TranslationDto> ParseTranslationNodes(HtmlNode node, string? language)
    {
        var languageFilter = language is null ? string.Empty : $" and @lang='{language}'";

        var nodes = node.SelectNodes($".//span[contains(concat(' ', normalize-space(@class), ' '), ' trans ') and contains(concat(' ', normalize-space(@class), ' '), ' dtrans '){languageFilter}]");

        if (nodes is null) return [];

        return nodes
            .Select(node => new TranslationDto
            {
                Language = node.GetAttributeValue("lang", language ?? string.Empty),
                Text = _normalizer.Text(node) ?? string.Empty
            })
            .Where(translation => !string.IsNullOrWhiteSpace(translation.Text))
            .DistinctBy(translation => translation.Text, StringComparer.OrdinalIgnoreCase);
    }
}
