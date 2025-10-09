using ChorukoAPI.Contracts.Responses;

namespace ChorukoAPI.Services.Abstraction;

public interface IAdminAuthService
{
    ServiceResponse<T> VerifySuperuserCredentials<T>(string providedUsername, string providedPassword);
}
