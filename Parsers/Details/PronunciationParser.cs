using DictionaryProvider.Api.Dtos.Dictionary;
using HtmlAgilityPack;

namespace DictionaryProvider.Api.Parsers.Details;

public class PronunciationParser
{
    private readonly HtmlTextNormalizer _normalizer;

    public PronunciationParser(HtmlTextNormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    public IEnumerable<PronunciationDto> Parse(IEnumerable<HtmlNode> entryNodes)
    {
        var pronunciations = new Dictionary<string, PronunciationDto>(StringComparer.OrdinalIgnoreCase);

        foreach (var entryNode in entryNodes)
        {
            AddPronunciation(entryNode, "uk", pronunciations);
            AddPronunciation(entryNode, "us", pronunciations);
        }

        return pronunciations.Values;
    }

    private void AddPronunciation(HtmlNode entryNode, string dialect, IDictionary<string, PronunciationDto> pronunciations)
    {
        if (pronunciations.ContainsKey(dialect)) return;

        var node = entryNode.SelectSingleNode($".//span[contains(concat(' ', normalize-space(@class), ' '), ' {dialect} ') and contains(concat(' ', normalize-space(@class), ' '), ' dpron-i ')]");

        if (node is null) return;

        var ipa = _normalizer.Text(node.SelectSingleNode(".//span[contains(concat(' ', normalize-space(@class), ' '), ' ipa ')]"));

        var audio = node
            .SelectSingleNode(".//source[@type='audio/mpeg']")
            ?.GetAttributeValue("src", string.Empty);

        if (ipa is null && string.IsNullOrWhiteSpace(audio)) return;

        pronunciations[dialect] = new PronunciationDto
        {
            Dialect = dialect,
            Ipa = ipa,
            AudioUrl = _normalizer.AbsoluteCambridgeUrl(audio)
        };
    }
}
