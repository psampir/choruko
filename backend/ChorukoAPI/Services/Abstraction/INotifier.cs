using ChorukoAPI.Contracts.Notifications;

namespace ChorukoAPI.Services.Abstraction;

public interface INotifier
{
    Task SendAsync(
        IEnumerable<string> userIds,
        AppNotification notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a test notification to one user and reports the outcome instead of
    /// only logging it.
    /// </summary>
    Task<NotificationTestResult> SendTestAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
