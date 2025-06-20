using System.Text.Json.Serialization;

namespace ChorukoServer.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationType : byte
{
    Settlement = 0,
    Task = 1,
    Invitation = 2
}
