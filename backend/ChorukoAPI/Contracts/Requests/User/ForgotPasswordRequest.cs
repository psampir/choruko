using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.User;

public class ForgotPasswordRequest
{
    [Required(ErrorMessage = "Email is required to initiate password reset.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;
}