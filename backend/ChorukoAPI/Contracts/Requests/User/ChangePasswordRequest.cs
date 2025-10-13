using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.User;

public record ChangePasswordRequest
{
    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public required string NewPassword { get; init; }

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
    public required string ConfirmNewPassword { get; init; }
}