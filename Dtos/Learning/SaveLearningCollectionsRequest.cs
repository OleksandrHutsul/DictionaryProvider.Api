namespace DictionaryProvider.Api.Dtos.Learning;

public class SaveLearningCollectionsRequest
{
    public IReadOnlyList<LearningCollectionDto> Collections { get; init; } = [];
}
