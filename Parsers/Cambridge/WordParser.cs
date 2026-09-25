using DictionaryProvider.Api.Dtos.Dictionary;
using DictionaryProvider.Api.Parsers.Details;
using HtmlAgilityPack;

namespace DictionaryProvider.Api.Parsers.Cambridge;

public class WordParser
{
    private const string ProviderName = "Cambridge";

    private readonly HtmlTextNormalizer _normalizer;
    private readonly PronunciationParser _pronunciationParser;
    private readonly MeaningParser _meaningParser;
    private readonly SynonymParser _synonymParser;
    private readonly AntonymParser _antonymParser;
    private readonly PhrasalVerbParser _phrasalVerbParser;
    private readonly IdiomParser _idiomParser;
    private readonly CollocationParser _collocationParser;

    public WordParser(HtmlTextNormalizer normalizer, PronunciationParser pronunciationParser, MeaningParser meaningParser, SynonymParser synonymParser, AntonymParser antonymParser, PhrasalVerbParser phrasalVerbParser, IdiomParser idiomParser, CollocationParser collocationParser)
    {
        _normalizer = normalizer;
        _pronunciationParser = pronunciationParser;
        _meaningParser = meaningParser;
        _synonymParser = synonymParser;
        _antonymParser = antonymParser;
        _phrasalVerbParser = phrasalVerbParser;
        _idiomParser = idiomParser;
        _collocationParser = collocationParser;
    }

    public DictionaryWordDto? Parse(string html, string dictionaryVariant, string? expectedWord = null)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var dictionaryNode = SelectDictionaryNode(document, dictionaryVariant);
        if (dictionaryNode is null) return null;

        if (!string.IsNullOrWhiteSpace(expectedWord))
        {
            var expectedEntry = SelectExpectedEntry(dictionaryNode, expectedWord);

            if (expectedEntry is not null)
                return ParseExpectedEntry(expectedEntry, expectedWord, dictionaryVariant);
        }

        return ParseDictionary(document, dictionaryNode, dictionaryVariant);
    }

    private DictionaryWordDto ParseExpectedEntry(HtmlNode entryNode, string expectedWord, string dictionaryVariant)
    {
        return new DictionaryWordDto
        {
            Word = expectedWord.ToLowerInvariant(),
            Provider = ProviderName,
            Origin = NormalizeOrigin(dictionaryVariant),
            Pronunciations = _pronunciationParser.Parse([entryNode]).ToArray(),
            Meanings = _meaningParser.Parse([entryNode]).ToArray(),
            Synonyms = _synonymParser.Parse(entryNode).ToArray(),
            Antonyms = _antonymParser.Parse(entryNode).ToArray(),
            PhrasalVerbs = _phrasalVerbParser.Parse(entryNode).ToArray(),
            Idioms = _idiomParser.Parse(entryNode).ToArray(),
            Collocations = _collocationParser.Parse(entryNode).ToArray()
        };
    }

    private DictionaryWordDto? ParseDictionary(HtmlDocument document, HtmlNode dictionaryNode, string dictionaryVariant)
    {
        var wordNode = document.DocumentNode.SelectSingleNode("//span[contains(concat(' ', normalize-space(@class), ' '), ' hw ') and contains(concat(' ', normalize-space(@class), ' '), ' dhw ')]");

        var word = _normalizer.Text(wordNode);
        if (string.IsNullOrWhiteSpace(word)) return null;

        var entryNodes = dictionaryNode.SelectNodes(".//div[contains(concat(' ', normalize-space(@class), ' '), ' pr ') and contains(concat(' ', normalize-space(@class), ' '), ' entry-body__el ')]")
            ?? Enumerable.Empty<HtmlNode>();

        return new DictionaryWordDto
        {
            Word = word.ToLowerInvariant(),
            Provider = ProviderName,
            Origin = NormalizeOrigin(dictionaryVariant),
            Pronunciations = _pronunciationParser.Parse(entryNodes).ToArray(),
            Meanings = _meaningParser.Parse(entryNodes).ToArray(),
            Synonyms = _synonymParser.Parse(dictionaryNode).ToArray(),
            Antonyms = _antonymParser.Parse(dictionaryNode).ToArray(),
            PhrasalVerbs = _phrasalVerbParser.Parse(dictionaryNode).ToArray(),
            Idioms = _idiomParser.Parse(dictionaryNode).ToArray(),
            Collocations = _collocationParser.Parse(dictionaryNode).ToArray()
        };
    }

    private HtmlNode? SelectExpectedEntry(HtmlNode dictionaryNode, string expectedWord)
    {
        var headwordNode = dictionaryNode
            .Descendants()
            .FirstOrDefault(node => node.NodeType == HtmlNodeType.Element && node.HasClass("headword") &&
                string.Equals(_normalizer.Text(node), expectedWord, StringComparison.OrdinalIgnoreCase));

        if (headwordNode is null) return null;

        return headwordNode
            .Ancestors()
            .FirstOrDefault(node => node.HasClass("idiom-block") || node.HasClass("pv-block"));
    }

    private static HtmlNode? SelectDictionaryNode(HtmlDocument document, string dictionaryVariant)
    {
        var dataId = dictionaryVariant.ToLowerInvariant() switch
        {
            "us" => "cacd",
            "be" or "business" => "cbed",
            _ => "cald4"
        };

        return document.DocumentNode.SelectSingleNode($"//div[@data-id='{dataId}']");
    }

    private static string NormalizeOrigin(string dictionaryVariant)
    {
        return dictionaryVariant.ToLowerInvariant() switch
        {
            "be" or "business" => "business",
            "us" => "us",
            _ => "uk"
        };
    }
}
