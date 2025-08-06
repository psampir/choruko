using System.Net;

namespace ChorukoAPI.Contracts.DTOs;

public class ServiceResponse<T>
{
    public T? Result { get; set; }
    public bool IsSuccessful { get; set; } = true;
    public string? ErrorMessage { get; set; } = string.Empty;
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
}
