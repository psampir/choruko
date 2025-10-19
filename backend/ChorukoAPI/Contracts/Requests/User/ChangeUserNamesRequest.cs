namespace ChorukoAPI.Contracts.Requests.User;
using System.ComponentModel.DataAnnotations;

public class ChangeUserNamesRequest
{
    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = string.Empty;
}