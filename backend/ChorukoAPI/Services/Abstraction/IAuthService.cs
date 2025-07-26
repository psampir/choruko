using ChorukoAPI.DTOs.User;
using Microsoft.AspNetCore.Identity;

namespace ChorukoAPI.Services.Abstraction
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterUserAsync(UserRegistrationDto registrationDto);
    }
}
