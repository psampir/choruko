using ChorukoAPI.Contracts.Notifications;
using ChorukoAPI.Services.Abstraction;

namespace ChorukoAPI.Services.Implementation;

public sealed class NoOpNotifier(ILogger<NoOpNotifier> logger) : INotifier
{
    public Task SendAsync(
        IEnumerable<string> userIds,
        AppNotification notification,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Notifications are disabled; skipping '{Title}'.", notification.Title);
        return Task.CompletedTask;
    }
}
