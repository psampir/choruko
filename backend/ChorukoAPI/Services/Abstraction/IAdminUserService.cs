using ChorukoAPI.Contracts.Requests.User;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.User;

namespace ChorukoAPI.Services.Abstraction;

public interface IAdminUserService
{
    Task<ServiceResponse<AdminResetPasswordResponse>> ResetUserPasswordAsync(AdminResetPasswordRequest request);
}
