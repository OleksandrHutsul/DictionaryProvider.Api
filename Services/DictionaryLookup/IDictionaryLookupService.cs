using DictionaryProvider.Api.Dtos.Dictionary;

namespace DictionaryProvider.Api.Services.DictionaryLookup;

public interface IDictionaryLookupService
{
    Task<DictionaryLookupResultDto> LookupAsync(string query, CancellationToken cancellationToken = default);
}
