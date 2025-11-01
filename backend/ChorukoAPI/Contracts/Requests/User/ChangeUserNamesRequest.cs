namespace ChorukoAPI.Contracts.Requests.User;
using System.ComponentModel.DataAnnotations;

public class ChangeUserNamesRequest
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(20, ErrorMessage = "First name is too long.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(20, ErrorMessage = "Last name is too long.")]
    public string LastName { get; set; } = string.Empty;
}