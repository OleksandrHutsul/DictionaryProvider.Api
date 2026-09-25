using DictionaryProvider.Api.Dtos.Dictionary;
using DictionaryProvider.Api.Parsers.Details;
using HtmlAgilityPack;

namespace DictionaryProvider.Api.Parsers.Cambridge;

public class MeaningParser
{
    private readonly HtmlTextNormalizer _normalizer;
    private readonly ExampleParser _exampleParser;
    private readonly TranslationParser _translationParser;
    private readonly SynonymParser _synonymParser;
    private readonly AntonymParser _antonymParser;

    public MeaningParser(HtmlTextNormalizer normalizer, ExampleParser exampleParser, TranslationParser translationParser, SynonymParser synonymParser, AntonymParser antonymParser)
    {
        _normalizer = normalizer;
        _exampleParser = exampleParser;
        _translationParser = translationParser;
        _synonymParser = synonymParser;
        _antonymParser = antonymParser;
    }

    public IEnumerable<DictionaryEntryDto> Parse(IEnumerable<HtmlNode> entryNodes)
    {
        var entries = new List<DictionaryEntryDto>();

        foreach (var entryNode in entryNodes)
        {
            var guideWordGroups = ParseGuideWordGroups(entryNode);
            if (guideWordGroups.Count == 0) continue;

            entries.Add(new DictionaryEntryDto
            {
                PartOfSpeech = ParsePartOfSpeech(entryNode),
                GuideWordGroups = guideWordGroups
            });
        }

        return entries;
    }

    private IReadOnlyList<GuideWordGroupDto> ParseGuideWordGroups(HtmlNode entryNode)
    {
        var senseNodes = entryNode.SelectNodes(".//div[contains(concat(' ', normalize-space(@class), ' '), ' dsense ')]")
            ?? entryNode.SelectNodes(".//div[contains(concat(' ', normalize-space(@class), ' '), ' dsense-noh ')]");

        if (senseNodes is null) return [];

        var groups = new List<GuideWordGroupDto>();

        foreach (var senseNode in senseNodes)
        {
            var meanings = ParseDefinitions(senseNode);
            if (meanings.Count == 0) continue;

            groups.Add(new GuideWordGroupDto
            {
                GuideWord = ParseGuideWord(senseNode),
                Meanings = meanings
            });
        }

        return groups;
    }

    private IReadOnlyList<MeaningDto> ParseDefinitions(HtmlNode senseNode)
    {
        var definitionNodes = senseNode.SelectNodes(".//div[contains(concat(' ', normalize-space(@class), ' '), ' def-block ') and contains(concat(' ', normalize-space(@class), ' '), ' ddef_block ')]");

        if (definitionNodes is null) return [];

        var meanings = new List<MeaningDto>();

        foreach (var definitionNode in definitionNodes)
        {
            var definition = ParseDefinition(definitionNode);
            if (definition is null) continue;

            meanings.Add(new MeaningDto
            {
                Definition = definition,
                CefrLevel = ParseCefrLevel(definitionNode),
                Examples = _exampleParser.Parse(definitionNode).ToArray(),
                Translations = _translationParser.Parse(definitionNode).ToArray(),
                Synonyms = _synonymParser.Parse(definitionNode).ToArray(),
                Antonyms = _antonymParser.Parse(definitionNode).ToArray()
            });
        }

        return meanings;
    }

    private string? ParseDefinition(HtmlNode definitionNode)
    {
        var definitionContainer = definitionNode.SelectSingleNode(".//div[contains(concat(' ', normalize-space(@class), ' '), ' def ') and contains(concat(' ', normalize-space(@class), ' '), ' ddef_d ')]");

        var definition = _normalizer.Text(definitionContainer?.SelectSingleNode(".//a[contains(concat(' ', normalize-space(@class), ' '), ' Ref ')]"))
            ?? _normalizer.Text(definitionContainer);

        var usage = _normalizer.Text(definitionNode.SelectSingleNode(".//span[contains(concat(' ', normalize-space(@class), ' '), ' usage ') and contains(concat(' ', normalize-space(@class), ' '), ' dusage ')]"));

        return usage is null ? definition : _normalizer.Clean($"{usage} {definition}");
    }

    private string? ParsePartOfSpeech(HtmlNode entryNode)
    {
        return _normalizer.Text(entryNode.SelectSingleNode(".//div[contains(concat(' ', normalize-space(@class), ' '), ' pos-header ') and contains(concat(' ', normalize-space(@class), ' '), ' dpos-h ')]//span[contains(concat(' ', normalize-space(@class), ' '), ' pos ') and contains(concat(' ', normalize-space(@class), ' '), ' dpos ')]"));
    }

    private string? ParseGuideWord(HtmlNode senseNode)
    {
        return _normalizer.Text(senseNode.SelectSingleNode(".//span[contains(concat(' ', normalize-space(@class), ' '), ' guideword ') and contains(concat(' ', normalize-space(@class), ' '), ' dsense_gw ')]/span"));
    }

    private string? ParseCefrLevel(HtmlNode definitionNode)
    {
        return _normalizer.Text(definitionNode.SelectSingleNode(".//span[contains(concat(' ', normalize-space(@class), ' '), ' epp-xref ') or contains(concat(' ', normalize-space(@class), ' '), ' dxref ')]"));
    }
}
