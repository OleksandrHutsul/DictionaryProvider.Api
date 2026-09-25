namespace DictionaryProvider.Api.Dtos.Dictionary;

public record CollocationDto
{
    public string Text { get; init; } = string.Empty;

    public IReadOnlyList<ExampleDto> Examples { get; init; } = [];
}
