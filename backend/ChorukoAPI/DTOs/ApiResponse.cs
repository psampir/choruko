namespace ChorukoAPI.DTOs;

public record ApiResponse<T> where T : class?
{
    public T? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
}