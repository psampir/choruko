using System.Text.Json.Serialization;

namespace ChorukoServer.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SettlementStatus : byte
{
    Pending = 0,
    Completed = 1
}