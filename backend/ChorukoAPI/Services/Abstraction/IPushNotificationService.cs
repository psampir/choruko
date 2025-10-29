using ChorukoAPI.Contracts.Requests.Notifications;

namespace ChorukoAPI.Services.Abstraction;

public interface IPushNotificationService
{
    Task<string> GetPublicKeyAsync();

    Task SubscribeAsync(string userId, PushSubscriptionRequest request);

    Task UnsubscribeAsync(string userId, string endpoint);

    Task SendNotificationAsync(string receiverId, string payloadJson);
}
