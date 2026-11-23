using System.Text.Json;

namespace ChorukoAPI.Contracts.Notifications;

public record AppNotification(string Title, string Body, string? Url = null, string[]? Tags = null)
{
    /// <summary>
    /// Builds a notification from the legacy Web Push JSON payload shape
    /// ({ title, body, icon, badge, url }) still produced by the services.
    /// </summary>
    public static AppNotification FromJson(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        var title = root.TryGetProperty("title", out var titleElement)
            ? titleElement.GetString()
            : null;
        var body = root.TryGetProperty("body", out var bodyElement)
            ? bodyElement.GetString()
            : null;
        var url = root.TryGetProperty("url", out var urlElement)
            ? urlElement.GetString()
            : null;

        return new AppNotification(title ?? "Choruko", body ?? string.Empty, url);
    }
}
