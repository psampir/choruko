namespace ChorukoAPI.Contracts.Responses;

public record ApiResponse<T> where T : class?
{
    public T? Result { get; set; }
    public string? Message { get; set; }
    public bool HasError => !string.IsNullOrEmpty(Message);
}