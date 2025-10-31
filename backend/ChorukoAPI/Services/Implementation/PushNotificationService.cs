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
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Endpoint = request.Endpoint,
                P256dhKey = request.P256dhKey,
                AuthKey = request.AuthKey,
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
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
        _logger.LogInformation("🔔 SendNotificationAsync called for user: {ReceiverId}", receiverId);
        
        var subscriptions = await _context.PushSubscriptions
            .Where(s => s.UserId == receiverId)
            .ToListAsync();

        _logger.LogInformation("Found {Count} subscription(s) for user {ReceiverId}", subscriptions.Count, receiverId);

        if (!subscriptions.Any())
        {
            _logger.LogWarning("❌ No push subscriptions found for user {ReceiverId}", receiverId);
            return;
        }

        // Log subscription details
        foreach (var sub in subscriptions)
        {
            _logger.LogInformation("  - Subscription: Endpoint={Endpoint}, Created={Created}", 
                sub.Endpoint.Substring(0, Math.Min(50, sub.Endpoint.Length)) + "...", 
                sub.CreatedAt);
        }

        var vapidDetails = new VapidDetails(
            _vapidSettings.Value.Subject,
            _vapidSettings.Value.PublicKey,
            _vapidSettings.Value.PrivateKey);

        _logger.LogInformation("VAPID Details: Subject={Subject}, PublicKey={PublicKey}", 
            _vapidSettings.Value.Subject, 
            _vapidSettings.Value.PublicKey.Substring(0, 20) + "...");

        var webPushClient = new WebPushClient();

        var subscriptionsToRemove = new ConcurrentBag<Models.PushSubscription>();

        var tasks = subscriptions.Select(sub =>
            Task.Run(async () =>
            {
                var pushSubscription = new WebPush.PushSubscription(sub.Endpoint, sub.P256dhKey, sub.AuthKey);
                try
                {
                    _logger.LogInformation("📤 Sending push to endpoint: {Endpoint}", sub.Endpoint.Substring(0, Math.Min(50, sub.Endpoint.Length)) + "...");
                    
                    await webPushClient.SendNotificationAsync(pushSubscription, payloadJson, vapidDetails);

                    _logger.LogInformation("✅ Successfully sent push to endpoint: {Endpoint}", sub.Endpoint.Substring(0, Math.Min(50, sub.Endpoint.Length)) + "...");

                    sub.LastUsedAt = DateTime.UtcNow;
                    _context.PushSubscriptions.Update(sub);
                }
                catch (WebPushException ex)
                {
                    _logger.LogError(ex, "❌ WebPush error for endpoint {Endpoint}. Status: {StatusCode}, Message: {Message}", 
                        sub.Endpoint.Substring(0, Math.Min(50, sub.Endpoint.Length)) + "...", 
                        ex.StatusCode, 
                        ex.Message);

                    if (ex.StatusCode == HttpStatusCode.Gone)
                    {
                        _logger.LogWarning("🗑️ Subscription expired (410 Gone), will remove: {Endpoint}", sub.Endpoint.Substring(0, Math.Min(50, sub.Endpoint.Length)) + "...");
                        subscriptionsToRemove.Add(sub);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Generic error sending push to {Endpoint}: {Message}", 
                        sub.Endpoint.Substring(0, Math.Min(50, sub.Endpoint.Length)) + "...", 
                        ex.Message);
                }
            })
        );

        await Task.WhenAll(tasks);

        if (!subscriptionsToRemove.IsEmpty)
        {
            _context.PushSubscriptions.RemoveRange(subscriptionsToRemove);
            _logger.LogInformation("🗑️ Removed {Count} expired subscriptions.", subscriptionsToRemove.Count);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("✅ SendNotificationAsync completed for user: {ReceiverId}", receiverId);
    }
}