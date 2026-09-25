namespace DictionaryProvider.Api.Dtos.Learning;

public class LearningCollectionsSnapshotDto
{
    public bool Initialized { get; init; }
    public IReadOnlyList<LearningCollectionDto> Collections { get; init; } = [];
}
