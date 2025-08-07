using ChorukoAPI.Contracts.DTOs;
using ChorukoAPI.Contracts.DTOs.User;
using ChorukoAPI.Contracts.Requests.User;
using Microsoft.AspNetCore.Identity;

namespace ChorukoAPI.Services.Abstraction;

public interface IAuthService
{
    Task<ServiceResponse<IdentityResult>> RegisterUserAsync(UserRegistrationRequest registrationDto);
    Task<ServiceResponse<SignInResult>> LoginUserAsync(UserLoginRequest loginDto);
    Task<ServiceResponse<UserInfoDto>> GetUserInfoAsync();
}
