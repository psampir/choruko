namespace ChorukoAPI.Contracts.Notifications;

public enum NotificationTestStatus
{
    Sent,
    Disabled,
    NotConfigured,
    NoTopic,
    Failed,
}

/// <summary>
/// Outcome of a test notification. Unlike regular deliveries, a test reports
/// failures to the caller so the user can see why nothing arrived.
/// </summary>
public record NotificationTestResult(NotificationTestStatus Status, string? Detail = null);
