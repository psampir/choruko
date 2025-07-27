using ChorukoAPI.DTOs.User;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ChorukoAPI.Services.Implementation;

public class AuthService(UserManager<User> _userManager, RoleManager<IdentityRole> _roleManager,
    SignInManager<User> _signInManager, IHttpContextAccessor _contextAccessor) : IAuthService
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

    public async Task<SignInResult> LoginUserAsync(UserLoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null) 
        {
            return SignInResult.Failed; 
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, loginDto.Password, isPersistent: false, lockoutOnFailure: false);

        return result;
    }

    public async Task<UserInfoDto> GetUserInfoAsync()
    {
        var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);

        if (user == null)
        {
            throw new Exception("User is not authenticated.");
        }

        return new UserInfoDto(user.FirstName, user.LastName, user.Email);
    }
}