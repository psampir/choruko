using ChorukoAPI.DTOs;
using ChorukoAPI.DTOs.User;
using Microsoft.AspNetCore.Identity;

namespace ChorukoAPI.Services.Abstraction;

public interface IAuthService
{
    Task<ServiceResponse<IdentityResult>> RegisterUserAsync(UserRegistrationDto registrationDto);
    Task<ServiceResponse<SignInResult>> LoginUserAsync(UserLoginDto loginDto);
    Task<ServiceResponse<UserInfoDto>> GetUserInfoAsync();
}
