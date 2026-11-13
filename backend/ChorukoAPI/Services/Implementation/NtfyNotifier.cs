using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Notifications;
using ChorukoAPI.Data;
using ChorukoAPI.Services.Abstraction;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ChorukoAPI.Services.Implementation;

public sealed class NtfyNotifier(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IOptions<NtfySettings> options,
    ILogger<NtfyNotifier> logger) : INotifier
{
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

        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        var topics = await context.Users
            .Where(user => ids.Contains(user.Id) && user.NtfyTopic != null && user.NtfyTopic != "")
            .Select(user => user.NtfyTopic!)
            .ToListAsync(cancellationToken);

        if (topics.Count == 0)
        {
            return;
        }

        var client = httpClientFactory.CreateClient("ntfy");
        var baseUrl = settings.BaseUrl.TrimEnd('/');
        var clickUrl = BuildClickUrl(settings.PublicAppUrl, notification.Url);

        foreach (var topic in topics)
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{baseUrl}/{Uri.EscapeDataString(topic)}")
                {
                    Content = JsonContent.Create(new
                    {
                        title = notification.Title,
                        message = notification.Body,
                        tags = notification.Tags,
                        click = clickUrl,
                    }),
                };

                if (!string.IsNullOrWhiteSpace(settings.Token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.Token);
                }

                using var response = await client.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning(
                        "ntfy returned {StatusCode} for topic {Topic}",
                        response.StatusCode,
                        topic);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "ntfy delivery failed for topic {Topic}", topic);
            }
        }
    }

    private static string? BuildClickUrl(string? appUrl, string? path)
    {
        if (string.IsNullOrWhiteSpace(appUrl) || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return $"{appUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }
}
