namespace DictionaryProvider.Api.Services.DictionaryQuery;

public interface IDictionaryQueryService
{
    string Normalize(string query);
    IReadOnlyList<string> GetCandidates(string query, bool baseFormFirst = true);
}
