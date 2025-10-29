using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.Notifications;

public class PushSubscriptionUnsubscribeRequest
{
    [Required]
    [StringLength(1024)]
    public string Endpoint { get; set; }
}
