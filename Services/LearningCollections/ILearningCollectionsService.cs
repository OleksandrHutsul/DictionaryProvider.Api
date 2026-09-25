using DictionaryProvider.Api.Dtos.Learning;

namespace DictionaryProvider.Api.Services.LearningCollections;

public interface ILearningCollectionsService
{
    Task<LearningCollectionsSnapshotDto> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task<LearningCollectionsSnapshotDto> SaveAsync(Guid userId, SaveLearningCollectionsRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid userId, Guid collectionId, CancellationToken cancellationToken);
}
