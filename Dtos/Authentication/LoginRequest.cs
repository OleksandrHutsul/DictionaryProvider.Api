using System.ComponentModel.DataAnnotations;

namespace DictionaryProvider.Api.Dtos.Authentication;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}
