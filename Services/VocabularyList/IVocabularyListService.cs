using DictionaryProvider.Api.Dtos.Learning;

namespace DictionaryProvider.Api.Services.VocabularyList;

public interface IVocabularyListService
{
    Task<IReadOnlyList<VocabularyListSummaryDto>> GetOwnedListsAsync(Guid userId, bool archived, CancellationToken cancellationToken);
    Task<IReadOnlyList<VocabularyListSummaryDto>> GetSharedListsAsync(Guid userId, CancellationToken cancellationToken);
    Task<VocabularyListDetailDto?> GetListAsync(Guid listId, Guid userId, CancellationToken cancellationToken);

    Task<VocabularyListDetailDto> CreateListAsync(Guid userId, CreateVocabularyListRequest request, CancellationToken cancellationToken);
    Task<VocabularyListDetailDto?> UpdateListAsync(Guid listId, Guid userId, UpdateVocabularyListRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteListAsync(Guid listId, Guid userId, CancellationToken cancellationToken);
    Task<bool> SetArchivedAsync(Guid listId, Guid userId, bool archived, CancellationToken cancellationToken);

    Task<VocabularyListWordDto?> AddWordAsync(Guid listId, Guid userId, AddVocabularyWordRequest request, CancellationToken cancellationToken);
    Task<bool> RemoveWordAsync(Guid listId, Guid wordId, Guid userId, CancellationToken cancellationToken);

    Task<VocabularyListShareDto?> ShareListAsync(Guid listId, Guid userId, ShareVocabularyListRequest request, CancellationToken cancellationToken);
    Task<bool> RemoveShareAsync(Guid listId, Guid sharedUserId, Guid userId, CancellationToken cancellationToken);

    Task<bool> SaveTestResultAsync(Guid listId, Guid userId, SaveListTestResultRequest request, CancellationToken cancellationToken);
}