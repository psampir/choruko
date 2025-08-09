using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using ChorukoAPI.Extensions;
using System.Net;
using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Requests.User;

namespace ChorukoAPI.Services.Implementation;

public class AuthService(UserManager<User> _userManager, RoleManager<IdentityRole> _roleManager,
    SignInManager<User> _signInManager, IHttpContextAccessor _contextAccessor) : IAuthService
{
    public async Task<ServiceResponse<IdentityResult>> RegisterUserAsync(UserRegistrationRequest request)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
        };

        var result = await _userManager.CreateAsync(user, request.Password);

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

    public async Task<ServiceResponse<SignInResult>> LoginUserAsync(UserLoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null) 
        {
            return ServiceResponseFactory.Error(SignInResult.Failed, "User not found", HttpStatusCode.NotFound);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, request.Password, isPersistent: false, lockoutOnFailure: false);

        return ServiceResponseFactory.Success(result, result.ToString());
    }

    public async Task<ServiceResponse<bool>> LogoutUserAsync()
    {
        var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext?.User);
        
        if (user == null)
        {
            return ServiceResponseFactory.Error<bool>(
                false,
                "User is not authenticated",
                HttpStatusCode.Unauthorized
                );
        }
        
        await _signInManager.SignOutAsync();
        
        var context = _contextAccessor.HttpContext;
        if (context != null)
        {
            foreach (var cookie in context.Request.Cookies)
            {
                if (cookie.Key.StartsWith(".AspNetCore") ||
                    cookie.Key.Contains("Auth") ||
                    cookie.Key.Contains("Identity"))
                {
                    context.Response.Cookies.Delete(cookie.Key);
                }
            }
        }
        
        return ServiceResponseFactory.Success<bool>(
            true,
            "User has been successfully logged out",
            HttpStatusCode.OK
            );
    }

    public async Task<ServiceResponse<UserInfoResponse>> GetUserInfoAsync()
    {
        var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);

        if (user == null)
        {
            return ServiceResponseFactory.Error(new UserInfoResponse("", "", ""), "User not found");
        }

        return ServiceResponseFactory.Success(new UserInfoResponse(user.FirstName, user.LastName, user.Email));
    }
}