using ChorukoAPI.Contracts.Requests.User;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.User;
using System.Threading.Tasks;
using ChorukoAPI.Contracts;

namespace ChorukoAPI.Services.Abstraction;

public interface IUserService
{
    Task<ServiceResponse<UserInfoResponse>> GetUserInfoAsync();
    Task<ServiceResponse<string>> GetOrCreateNotificationTopicAsync(string userId);
    Task<ServiceResponse<OperationResultDto>> ChangeUserNamesAsync(ChangeUserNamesRequest request, string userId);
    Task<ServiceResponse<OperationResultDto>> ChangeUserProfilePictureAsync(ChangeUserProfilePictureRequest request, string userId);
    Task<ServiceResponse<OperationResultDto>> DeleteSelfAsync(string userId);
    Task<ServiceResponse<OperationResultDto>> ChangePasswordAsync(ChangePasswordRequest request, string userId);
}