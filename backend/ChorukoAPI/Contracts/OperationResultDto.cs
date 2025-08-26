namespace ChorukoAPI.Contracts;

public record OperationResultDto
{
    public string Operation { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; init; }
}
