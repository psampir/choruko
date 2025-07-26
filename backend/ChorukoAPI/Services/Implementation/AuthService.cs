using ChorukoAPI.DTOs.User;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ChorukoAPI.Services.Implementation;

public class AuthService(UserManager<User> _userManager, RoleManager<IdentityRole> _roleManager) : IAuthService
{
    public async Task<IdentityResult> RegisterUserAsync(UserRegistrationDto registrationDto)
    {
        var user = new User
        {
            UserName = registrationDto.Email,
            Email = registrationDto.Email,
            FirstName = registrationDto.FirstName,
            LastName = registrationDto.LastName,
        };

        var result = await _userManager.CreateAsync(user, registrationDto.Password);

        if (result.Succeeded)
        {
            if (!await _roleManager.RoleExistsAsync("None"))
            {
                await _roleManager.CreateAsync(new IdentityRole("None"));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "None");
            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    result.Errors.Append(error);
                }
            }
        }

        return result;
    }
}