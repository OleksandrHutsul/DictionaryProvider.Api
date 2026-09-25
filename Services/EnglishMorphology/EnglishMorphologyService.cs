namespace DictionaryProvider.Api.Services.EnglishMorphology;

public class EnglishMorphologyService : IEnglishMorphologyService
{
    private static readonly IReadOnlyList<(string Suffix, string Replacement)> NounRules =
    [
        ("ches", "ch"),
        ("shes", "sh"),
        ("ses", "s"),
        ("xes", "x"),
        ("zes", "z"),
        ("men", "man"),
        ("ies", "y"),
        ("s", "")
    ];

    private static readonly IReadOnlyList<(string Suffix, string Replacement)> VerbRules =
    [
        ("ies", "y"),
        ("ing", "e"),
        ("ing", ""),
        ("ed", "e"),
        ("ed", ""),
        ("es", "e"),
        ("es", ""),
        ("s", "")
    ];

    private static readonly IReadOnlyList<(string Suffix, string Replacement)> AdjectiveRules =
    [
        ("est", ""),
        ("est", "e"),
        ("er", ""),
        ("er", "e")
    ];

    private readonly Dictionary<string, string[]> _exceptions = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _nouns = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _verbs = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _adjectives = new(StringComparer.OrdinalIgnoreCase);

    public EnglishMorphologyService(IWebHostEnvironment environment)
    {
        var wordNetPath = Path.Combine(environment.ContentRootPath, "Data", "WordNet");

        LoadExceptions(Path.Combine(wordNetPath, "noun.exc"));
        LoadExceptions(Path.Combine(wordNetPath, "verb.exc"));
        LoadExceptions(Path.Combine(wordNetPath, "adj.exc"));
        LoadExceptions(Path.Combine(wordNetPath, "adv.exc"));

        LoadIndex(Path.Combine(wordNetPath, "index.noun"), _nouns);
        LoadIndex(Path.Combine(wordNetPath, "index.verb"), _verbs);
        LoadIndex(Path.Combine(wordNetPath, "index.adj"), _adjectives);
    }

    public IReadOnlyList<string> GetBaseForms(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return [];

        var normalizedWord = word.Trim().ToLowerInvariant();
        var forms = new List<string>();

        if (_exceptions.TryGetValue(normalizedWord, out var exceptions))
            forms.AddRange(exceptions);

        AddForms(normalizedWord, NounRules, _nouns, forms);
        AddForms(normalizedWord, VerbRules, _verbs, forms);
        AddForms(normalizedWord, AdjectiveRules, _adjectives, forms);

        return forms
            .Where(form => !form.Equals(normalizedWord, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void LoadExceptions(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"WordNet exception file was not found: {path}");

        foreach (var line in File.ReadLines(path))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;

            var word = NormalizeWordNetValue(parts[0]);
            var forms = parts[1..].Select(NormalizeWordNetValue).ToArray();

            if (_exceptions.TryGetValue(word, out var existing))
            {
                _exceptions[word] = existing
                    .Concat(forms)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            else
            {
                _exceptions[word] = forms;
            }
        }
    }

    private static void LoadIndex(string path, HashSet<string> lemmas)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"WordNet index file was not found: {path}");

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line) || char.IsWhiteSpace(line[0])) continue;

            var separator = line.IndexOf(' ');
            if (separator <= 0) continue;

            var lemma = NormalizeWordNetValue(line[..separator]);
            if (lemma.Length > 0) lemmas.Add(lemma);
        }
    }

    private static void AddForms(string word, IReadOnlyList<(string Suffix, string Replacement)> rules, HashSet<string> lemmas, List<string> forms)
    {
        foreach (var (suffix, replacement) in rules)
        {
            if (!word.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) || word.Length <= suffix.Length) continue;

            var candidate = word[..^suffix.Length] + replacement;

            if (lemmas.Contains(candidate))
                forms.Add(candidate);
        }
    }

    private static string NormalizeWordNetValue(string value)
    {
        return value.Replace('_', ' ');
    }
}
