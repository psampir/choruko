using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.User;

public record UserLoginRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(256, ErrorMessage = "Email is too long")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
