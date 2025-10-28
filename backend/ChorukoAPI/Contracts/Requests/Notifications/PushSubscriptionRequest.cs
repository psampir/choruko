using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.Notifications;

public class PushSubscriptionRequest
{
    [Required]
    public string Endpoint { get; set; }

    [Required]
    public string P256dhKey { get; set; }

    [Required]
    public string AuthKey { get; set; }
}
