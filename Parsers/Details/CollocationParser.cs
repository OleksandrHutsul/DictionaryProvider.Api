using DictionaryProvider.Api.Dtos.Dictionary;
using HtmlAgilityPack;

namespace DictionaryProvider.Api.Parsers.Details;

public class CollocationParser
{
    private readonly HtmlTextNormalizer _normalizer;

    public CollocationParser(HtmlTextNormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    public IEnumerable<CollocationDto> Parse(HtmlNode node)
    {
        var nodes = node.SelectNodes(".//span[contains(concat(' ', normalize-space(@class), ' '), ' collocation ') or contains(concat(' ', normalize-space(@class), ' '), ' dcoll ')]");

        if (nodes is null) return [];

        return nodes
            .Select(node => new CollocationDto
            {
                Text = _normalizer.Text(node) ?? string.Empty
            })
            .Where(collocation => !string.IsNullOrWhiteSpace(collocation.Text))
            .DistinctBy(collocation => collocation.Text, StringComparer.OrdinalIgnoreCase);
    }
}
