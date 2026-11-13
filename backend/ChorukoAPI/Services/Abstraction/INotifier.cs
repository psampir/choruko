using ChorukoAPI.Contracts.Notifications;

namespace ChorukoAPI.Services.Abstraction;

public interface INotifier
{
    Task SendAsync(
        IEnumerable<string> userIds,
        AppNotification notification,
        CancellationToken cancellationToken = default);
}
