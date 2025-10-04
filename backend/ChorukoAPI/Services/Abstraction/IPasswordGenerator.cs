using Microsoft.AspNetCore.Identity;

namespace ChorukoAPI.Services.Abstraction;

public interface IPasswordGenerator
{
    string GenerateCompliantPassword(PasswordOptions options);
}
