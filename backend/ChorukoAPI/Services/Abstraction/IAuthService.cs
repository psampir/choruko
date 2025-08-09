using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Contracts.Requests.User;
using Microsoft.AspNetCore.Identity;
using ChorukoAPI.Contracts;

namespace ChorukoAPI.Services.Abstraction;

public interface IAuthService
{
    Task<ServiceResponse<IdentityResult>> RegisterUserAsync(UserRegistrationRequest request);
    Task<ServiceResponse<SignInResult>> LoginUserAsync(UserLoginRequest request);
    Task<ServiceResponse<UserInfoResponse>> GetUserInfoAsync();
    Task<ServiceResponse<OperationResultDto>> LogoutUserAsync();
}
