using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Notifications;
using ChorukoAPI.Contracts.Responses;

namespace ChorukoAPI.Services.Abstraction;

public interface IPushNotificationService
{
    Task<ServiceResponse<string>> GetPublicKeyAsync();

    Task<ServiceResponse<OperationResultDto>> SubscribeAsync(string userId, PushSubscriptionRequest request);

    Task<ServiceResponse<OperationResultDto>> UnsubscribeAsync(string userId, string endpoint);

    Task<ServiceResponse<OperationResultDto>> SendNotificationAsync(string receiverId, string payloadJson);
}
