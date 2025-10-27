namespace ChorukoAPI.Models;

public class PushSubscription
{
    public string Id { get; set; }

    public string UserId { get; set; }

    public string Endpoint { get; set; }

    public string P256dhKey { get; set; }

    public string AuthKey { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastUsedAt { get; set; }
}
