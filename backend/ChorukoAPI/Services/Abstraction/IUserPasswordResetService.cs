using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.User;

namespace ChorukoAPI.Services.Abstraction;

public interface IUserPasswordResetService
{
    Task<ServiceResponse<UserPasswordResetData>> ResetPasswordAsync(string email);
}
