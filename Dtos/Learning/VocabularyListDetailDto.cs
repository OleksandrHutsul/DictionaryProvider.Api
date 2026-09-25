namespace DictionaryProvider.Api.Dtos.Learning;

public class VocabularyListDetailDto : VocabularyListSummaryDto
{
    public IReadOnlyList<VocabularyListWordDto> Words { get; init; } = [];
    public IReadOnlyList<VocabularyListShareDto> Shares { get; init; } = [];
}
