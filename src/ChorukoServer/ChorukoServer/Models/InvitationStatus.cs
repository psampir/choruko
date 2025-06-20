using System.Text.Json.Serialization;

namespace ChorukoServer.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InvitationStatus : byte
{
    Pending = 0,
    Accepted = 1,
    Declined = 2
}
