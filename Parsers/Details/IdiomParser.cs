using DictionaryProvider.Api.Dtos.Dictionary;
using HtmlAgilityPack;

namespace DictionaryProvider.Api.Parsers.Details;

public class IdiomParser
{
    private readonly HtmlTextNormalizer _normalizer;

    public IdiomParser(HtmlTextNormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    public IEnumerable<RelatedTermDto> Parse(HtmlNode node)
    {
        var nodes = node.SelectNodes(".//div[contains(concat(' ', normalize-space(@class), ' '), ' xref ') and contains(concat(' ', normalize-space(@class), ' '), ' idioms ')]//a[.//span[contains(concat(' ', normalize-space(@class), ' '), ' x-h ')]]");

        if (nodes is null) return [];

        return nodes
            .Select(ToRelatedTerm)
            .Where(term => !string.IsNullOrWhiteSpace(term.Text))
            .DistinctBy(term => term.Text, StringComparer.OrdinalIgnoreCase);
    }

    private RelatedTermDto ToRelatedTerm(HtmlNode node)
    {
        var textNode = node.SelectSingleNode(".//span[contains(concat(' ', normalize-space(@class), ' '), ' x-h ')]");

        return new RelatedTermDto
        {
            Text = _normalizer.Text(textNode) ?? string.Empty,
            Url = _normalizer.AbsoluteCambridgeUrl(node.GetAttributeValue("href", string.Empty))
        };
    }
}
