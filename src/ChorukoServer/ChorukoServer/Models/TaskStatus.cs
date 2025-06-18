using System.Text.Json.Serialization;

namespace ChorukoServer.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskStatus : byte
{
    Pending = 0,
    Assigned = 1,
    Completed = 2
}
