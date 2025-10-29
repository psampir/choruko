using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Notifications;
using ChorukoAPI.Data;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;
using WebPush;

namespace ChorukoAPI.Services.Implementation;

public class PushNotificationService(
    DataContext _context,
    IOptions<VapidSettings> _vapidSettings,
    ILogger<PushNotificationService> _logger) : IPushNotificationService
{
    public Task<string> GetPublicKeyAsync()
    {
        return Task.FromResult(_vapidSettings.Value.PublicKey);
    }

    public async Task SubscribeAsync(string userId, PushSubscriptionRequest request)
    {
        var existingSubscription = await _context.PushSubscriptions
            .FirstOrDefaultAsync(s => s.Endpoint == request.Endpoint);

        if (existingSubscription == null)
        {
            var newSubscription = new Models.PushSubscription
            {
                UserId = userId,
                Endpoint = request.Endpoint,
                P256dhKey = request.P256dhKey,
                AuthKey = request.AuthKey,
                CreatedAt = DateTime.UtcNow
            };

            await _context.PushSubscriptions.AddAsync(newSubscription);
            _logger.LogInformation("New push subscription added for user {UserId}", userId);
        }
        else
        {
            existingSubscription.UserId = userId;
            _context.PushSubscriptions.Update(existingSubscription);
            _logger.LogInformation("Existing push subscription re-associated for user {UserId}", userId);
        }

        await _context.SaveChangesAsync();
    }

    public async Task UnsubscribeAsync(string userId, string endpoint)
    {
        var subscription = await _context.PushSubscriptions
            .FirstOrDefaultAsync(s => s.Endpoint == endpoint && s.UserId == userId);

        if (subscription != null)
        {
            _context.PushSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Push subscription removed for user {UserId}", userId);
        }
    }

    public async Task SendNotificationAsync(string receiverId, string payloadJson)
    {
        var subscriptions = await _context.PushSubscriptions
            .Where(s => s.UserId == receiverId)
            .ToListAsync();

        if (!subscriptions.Any())
        {
            _logger.LogWarning("No push subscriptions found for user {ReceiverId}", receiverId);
            return;
        }

        var vapidDetails = new VapidDetails(
            _vapidSettings.Value.Subject,
            _vapidSettings.Value.PublicKey,
            _vapidSettings.Value.PrivateKey);

        var webPushClient = new WebPushClient();

        var subscriptionsToRemove = new ConcurrentBag<Models.PushSubscription>();

        var tasks = subscriptions.Select(sub =>
            Task.Run(async () =>
            {
                var pushSubscription = new WebPush.PushSubscription(sub.Endpoint, sub.P256dhKey, sub.AuthKey);
                try
                {
                    await webPushClient.SendNotificationAsync(pushSubscription, payloadJson, vapidDetails);

                    sub.LastUsedAt = DateTime.UtcNow;
                    _context.PushSubscriptions.Update(sub);
                }
                catch (WebPushException ex)
                {
                    _logger.LogError(ex, "WebPush error for endpoint {Endpoint}. Status: {StatusCode}", sub.Endpoint, ex.StatusCode);

                    if (ex.StatusCode == HttpStatusCode.Gone)
                    {
                        subscriptionsToRemove.Add(sub);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Generic error sending push to {Endpoint}", sub.Endpoint);
                }
            })
        );

        await Task.WhenAll(tasks);

        if (!subscriptionsToRemove.IsEmpty)
        {
            _context.PushSubscriptions.RemoveRange(subscriptionsToRemove);
            _logger.LogInformation("Removed {Count} expired subscriptions.", subscriptionsToRemove.Count);
        }

        await _context.SaveChangesAsync();
    }
}