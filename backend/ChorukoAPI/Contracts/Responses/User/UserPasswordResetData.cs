using ChorukoAPI.Models;

namespace ChorukoAPI.Contracts.Responses.User;

public class UserPasswordResetData
{
    public Models.User User { get; set; } = null!;
    public string NewPassword { get; set; } = string.Empty;
}
