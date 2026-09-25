using DictionaryProvider.Api.Dtos.Dictionary;
using HtmlAgilityPack;

namespace DictionaryProvider.Api.Parsers.Details;

public class SynonymParser
{
    private readonly HtmlTextNormalizer _normalizer;

    public SynonymParser(HtmlTextNormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    public IEnumerable<RelatedTermDto> Parse(HtmlNode node)
    {
        var nodes = node.SelectNodes(".//*[contains(translate(normalize-space(.), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'synonym')]/following::a[contains(concat(' ', normalize-space(@class), ' '), ' daccord_h ') or contains(concat(' ', normalize-space(@class), ' '), ' x-h ')]");

        if (nodes is null) return [];

        return nodes
            .Select(ToRelatedTerm)
            .Where(term => !string.IsNullOrWhiteSpace(term.Text))
            .DistinctBy(term => term.Text, StringComparer.OrdinalIgnoreCase);
    }

    private RelatedTermDto ToRelatedTerm(HtmlNode node)
    {
        return new RelatedTermDto
        {
            Text = _normalizer.Text(node) ?? string.Empty,
            Url = _normalizer.AbsoluteCambridgeUrl(node.GetAttributeValue("href", string.Empty))
        };
    }
}
