using System.ComponentModel.DataAnnotations;

namespace DictionaryProvider.Api.Dtos.Authentication;

public class RegisterRequest
{
    [Required]
    [MaxLength(80)]
    public string UserName { get; set; } = "";

    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = "";

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = "";
}
