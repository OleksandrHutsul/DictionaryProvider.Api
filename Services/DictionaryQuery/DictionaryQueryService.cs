using System.Text.RegularExpressions;
using DictionaryProvider.Api.Services.EnglishMorphology;

namespace DictionaryProvider.Api.Services.DictionaryQuery;

public class DictionaryQueryService : IDictionaryQueryService
{
    private readonly IEnglishMorphologyService _morphologyService;

    public DictionaryQueryService(IEnglishMorphologyService morphologyService)
    {
        _morphologyService = morphologyService;
    }

    public string Normalize(string query)
    {
        return string.Join(' ', SplitWords(query));
    }

    public IReadOnlyList<string> GetCandidates(string query, bool baseFormFirst = true)
    {
        var phrase = Normalize(query);
        if (phrase.Length == 0) return [];

        var words = SplitWords(phrase);
        var baseWords = words.ToArray();

        for (var i = 0; i < words.Length; i++)
        {
            var baseForm = _morphologyService.GetBaseForms(words[i]).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(baseForm)) baseWords[i] = baseForm;
        }

        var basePhrase = string.Join(' ', baseWords);

        if (basePhrase.Equals(phrase, StringComparison.OrdinalIgnoreCase))
            return [phrase];

        return baseFormFirst
            ? [basePhrase, phrase]
            : [phrase, basePhrase];
    }

    private static string[] SplitWords(string value)
    {
        return Regex.Replace(value ?? "", @"[!?.,;:""“”\-–—]+", " ").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
    }
}
