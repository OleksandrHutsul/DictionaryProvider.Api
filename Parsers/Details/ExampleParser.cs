using DictionaryProvider.Api.Dtos.Dictionary;
using HtmlAgilityPack;

namespace DictionaryProvider.Api.Parsers.Details;

public class ExampleParser
{
    private readonly HtmlTextNormalizer _normalizer;

    public ExampleParser(HtmlTextNormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    public IEnumerable<ExampleDto> Parse(HtmlNode definitionNode)
    {
        var examples = SelectInlineExamples(definitionNode).ToList();

        if (examples.Count == 0 && definitionNode.ParentNode is not null)
            examples.AddRange(SelectAccordionExamples(definitionNode.ParentNode));

        return examples
            .Where(example => !string.IsNullOrWhiteSpace(example))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(example => new ExampleDto { Text = example! });
    }

    private IEnumerable<string?> SelectInlineExamples(HtmlNode definitionNode)
    {
        var nodes = definitionNode.SelectNodes(".//span[contains(concat(' ', normalize-space(@class), ' '), ' eg ') or contains(concat(' ', normalize-space(@class), ' '), ' deg ')]");

        if (nodes is null) return [];

        return nodes.Select(_normalizer.Text);
    }

    private IEnumerable<string?> SelectAccordionExamples(HtmlNode senseNode)
    {
        var nodes = senseNode.SelectNodes(".//li[contains(concat(' ', normalize-space(@class), ' '), ' eg ') and contains(concat(' ', normalize-space(@class), ' '), ' dexamp ')]");

        if (nodes is null) return [];

        return nodes.Select(_normalizer.Text);
    }
}
