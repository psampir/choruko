using System.Text.Json.Serialization;

namespace ChorukoAPI.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SettlementStatus : byte
{
    Pending = 0,
    Completed = 1
}