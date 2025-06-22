using System.Text.Json.Serialization;

namespace ChorukoAPI.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InvitationStatus : byte
{
    Pending = 0,
    Accepted = 1,
    Declined = 2
}
