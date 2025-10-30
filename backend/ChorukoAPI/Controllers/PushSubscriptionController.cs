using ChorukoAPI.Contracts.Requests.Notifications;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChorukoAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
public class PushSubscriptionController(IPushNotificationService pushService) : ControllerBase
{
    [HttpGet("public-key")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicKey()
    {
        var publicKey = await pushService.GetPublicKeyAsync();

        return Ok(new { publicKey });
    }

    [HttpPost("subscribe")]
    [Authorize]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionRequest dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User ID not found in token." });
        }

        await pushService.SubscribeAsync(userId, dto);

        return Ok(new { message = "Subscription saved." });
    }

    [HttpDelete("unsubscribe")]
    [Authorize]
    public async Task<IActionResult> Unsubscribe([FromBody] PushSubscriptionUnsubscribeRequest request )
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User ID not found in token." });
        }

        await pushService.UnsubscribeAsync(userId, request.Endpoint);

        return Ok(new { message = "Subscription removed." });
    }

    [HttpPost("test")]
    [Authorize]
    public async Task<IActionResult> SendTestNotification()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User ID not found in token." });
        }

        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "🎉 Test Notification",
            body = "This is a test push notification from Choruko!",
            icon = "/icons/icon-192x192.png",
            badge = "/icons/icon-96x96.png",
            url = "/test-notifications"
        });

        await pushService.SendNotificationAsync(userId, payload);

        return Ok(new { message = "Test notification sent!" });
    }
}