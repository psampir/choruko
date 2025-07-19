using ChorukoAPI.DTOs;
using ChorukoAPI.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ChorukoAPI.Services;

public class AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    
    public async Task<IdentityResult> RegisterUserAsync(UserRegistrationDto registrationDto)
    {
        var user = new User
        {
            UserName = registrationDto.Email,
            Email = registrationDto.Email,
            FirstName = registrationDto.FirstName,
            LastName = registrationDto.LastName,
        };

        // Attempt to create the user with the provided password
        var result = await _userManager.CreateAsync(user, registrationDto.Password);

        if (result.Succeeded)
        {
            // Ensure the "None" role exists
            if (!await _roleManager.RoleExistsAsync("None"))
            {
                await _roleManager.CreateAsync(new IdentityRole("None"));
            }

            // Assign the "None" role to the newly registered user
            var roleResult = await _userManager.AddToRoleAsync(user, "None");
            if (!roleResult.Succeeded)
            {
                // If role assignment fails, you might want to log this or handle it
                // For now, we'll add the errors to the main result, though CreateAsync already succeeded.
                foreach (var error in roleResult.Errors)
                {
                    result.Errors.Append(error);
                }
            }
        }

        return result;
    }
}