using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using ChorukoAPI.Extensions;
using System.Net;
using ChorukoAPI.Contracts.DTOs.User;
using ChorukoAPI.Contracts.DTOs;
using ChorukoAPI.Contracts.Requests.User;

namespace ChorukoAPI.Services.Implementation;

public class AuthService(UserManager<User> _userManager, RoleManager<IdentityRole> _roleManager,
    SignInManager<User> _signInManager, IHttpContextAccessor _contextAccessor) : IAuthService
{
    public async Task<ServiceResponse<IdentityResult>> RegisterUserAsync(UserRegistrationRequest registrationDto)
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

                return ServiceResponseFactory.Error(result, result.ToString());
            }

            return ServiceResponseFactory.Success(result, result.ToString());
        }

        return ServiceResponseFactory.Error(result, result.ToString());
    }

    public async Task<ServiceResponse<SignInResult>> LoginUserAsync(UserLoginRequest loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null) 
        {
            return ServiceResponseFactory.Error(SignInResult.Failed, "User not found", HttpStatusCode.NotFound);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, loginDto.Password, isPersistent: false, lockoutOnFailure: false);

        return ServiceResponseFactory.Success(result, result.ToString());
    }

    public async Task<ServiceResponse<UserInfoDto>> GetUserInfoAsync()
    {
        var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);

        if (user == null)
        {
            return ServiceResponseFactory.Error(new UserInfoDto("", "", ""), "User not found");
        }

        return ServiceResponseFactory.Success(new UserInfoDto(user.FirstName, user.LastName, user.Email));
    }
}