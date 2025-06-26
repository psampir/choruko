using System.Text.Json.Serialization;

namespace ChorukoAPI.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskStatus : byte
{
    Unassigned = 0,
    Assigned = 1,
    Completed = 2
}
