namespace DictionaryProvider.Api.Services.EnglishMorphology;

public interface IEnglishMorphologyService
{
    IReadOnlyList<string> GetBaseForms(string word);
}
