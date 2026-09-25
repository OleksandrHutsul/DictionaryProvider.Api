namespace DictionaryProvider.Api.Dtos.Learning;

public class SaveListTestResultRequest
{
    public int CorrectAnswers { get; init; }
    public int TotalQuestions { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
}
