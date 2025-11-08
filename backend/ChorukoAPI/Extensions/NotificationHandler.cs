using ChorukoAPI.Services.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace ChorukoAPI.Extensions;

public static class NotificationHandler
{
    private static IServiceProvider _serviceProvider;

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static async Task SendNotificationAsync(IEnumerable<string> receiverIds, string payload)
    {
        if (_serviceProvider == null)
        {
            throw new InvalidOperationException("NotificationHandler is not initialized.");
        }

        if (receiverIds == null || !receiverIds.Any())
        {
            return;
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var pushService = scope.ServiceProvider.GetRequiredService<IPushNotificationService>();

            var notificationTasks = receiverIds
                .Select(receiverId => pushService.SendNotificationAsync(receiverId, payload))
                .ToList();

            await Task.WhenAll(notificationTasks);
        }
    }
}