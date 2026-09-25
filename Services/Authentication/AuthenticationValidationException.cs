namespace DictionaryProvider.Api.Services.Authentication;

public class AuthenticationValidationException : Exception
{
    public string Field { get; }

    public AuthenticationValidationException(string field, string message) : base(message)
    {
        Field = field;
    }
}
