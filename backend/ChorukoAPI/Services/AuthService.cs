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