using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Notifications;
using ChorukoAPI.Data;
using ChorukoAPI.Services.Abstraction;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChorukoAPI.Services.Implementation;

public sealed class NtfyNotifier(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IOptions<NtfySettings> options,
    ILogger<NtfyNotifier> logger) : INotifier
{
    // ntfy rejects null fields such as "tags": null, so leave them out.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private static readonly AppNotification TestNotification = new(
        "🔔 Test notification",
        "Notifications from Choruko work on this device.",
        "/manageAccount");

    public async Task SendAsync(
        IEnumerable<string> userIds,
        AppNotification notification,
        CancellationToken cancellationToken = default)
    {
        var ids = userIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToArray();

        if (ids.Length == 0)
        {
            return;
        }

        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            logger.LogWarning("NTFY_BASE_URL is not configured; notification skipped.");
            return;
        }

        var topics = await GetTopicsAsync(ids, cancellationToken);

        foreach (var topic in topics)
        {
            var delivery = await PostAsync(topic, notification, cancellationToken);
            if (delivery != null)
            {
                logger.LogWarning("ntfy delivery failed for topic {Topic}: {Detail}", topic, delivery);
            }
        }
    }

    public async Task<NotificationTestResult> SendTestAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.Value.BaseUrl))
        {
            return new NotificationTestResult(NotificationTestStatus.NotConfigured);
        }

        var topic = (await GetTopicsAsync([userId], cancellationToken)).FirstOrDefault();
        if (topic == null)
        {
            return new NotificationTestResult(NotificationTestStatus.NoTopic);
        }

        var failure = await PostAsync(topic, TestNotification, cancellationToken);
        if (failure != null)
        {
            logger.LogWarning("ntfy test notification failed for topic {Topic}: {Detail}", topic, failure);
            return new NotificationTestResult(NotificationTestStatus.Failed, failure);
        }

        return new NotificationTestResult(NotificationTestStatus.Sent);
    }

    private async Task<List<string>> GetTopicsAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        return await context.Users
            .Where(user => userIds.Contains(user.Id) && user.NtfyTopic != null && user.NtfyTopic != "")
            .Select(user => user.NtfyTopic!)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Posts one message to a topic. ntfy only parses JSON published to the server
    /// root with a "topic" field; JSON sent to /{topic} is delivered verbatim as the
    /// message text. Returns null on success, otherwise a short failure description.
    /// </summary>
    private async Task<string?> PostAsync(
        string topic,
        AppNotification notification,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var client = httpClientFactory.CreateClient("ntfy");
        var baseUrl = settings.BaseUrl.TrimEnd('/');

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/")
            {
                Content = JsonContent.Create(
                    new NtfyMessage(
                        topic,
                        notification.Title,
                        notification.Body,
                        notification.Tags,
                        BuildClickUrl(settings.PublicAppUrl, notification.Url)),
                    options: JsonOptions),
            };

            if (!string.IsNullOrWhiteSpace(settings.Token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.Token);
            }

            using var response = await client.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode
                ? null
                : $"ntfy returned {(int)response.StatusCode} {response.StatusCode}";
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return ex.Message;
        }
    }

    private sealed record NtfyMessage(
        [property: JsonPropertyName("topic")] string Topic,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("tags")] string[]? Tags,
        [property: JsonPropertyName("click")] string? Click);

    private static string? BuildClickUrl(string? appUrl, string? path)
    {
        if (string.IsNullOrWhiteSpace(appUrl) || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return $"{appUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }
}
