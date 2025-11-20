using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.Notifications;

public class PushSubscriptionRequest
{
    [Required]
    public required string Endpoint { get; set; }

    [Required]
    public required string P256dhKey { get; set; }

    [Required]
    public required string AuthKey { get; set; }
}
