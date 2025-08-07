using System.Net;

namespace ChorukoAPI.Contracts.Responses;

public class ServiceResponse<T>
{
    public T? Result { get; set; }
    public bool IsSuccessful { get; set; } = true;
    public string? Message { get; set; } = string.Empty;
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
}
