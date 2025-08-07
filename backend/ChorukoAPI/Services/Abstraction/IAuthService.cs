using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Contracts.Requests.User;
using Microsoft.AspNetCore.Identity;

namespace ChorukoAPI.Services.Abstraction;

public interface IAuthService
{
    Task<ServiceResponse<IdentityResult>> RegisterUserAsync(UserRegistrationRequest request);
    Task<ServiceResponse<SignInResult>> LoginUserAsync(UserLoginRequest request);
    Task<ServiceResponse<UserInfoResponse>> GetUserInfoAsync();
}
