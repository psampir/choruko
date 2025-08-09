using ChorukoAPI.Contracts.Responses;
using System.Net;

namespace ChorukoAPI.Extensions;

public static class ServiceResponseFactory
{
    public static ServiceResponse<T> Error<T>(T data, string message, HttpStatusCode code = HttpStatusCode.InternalServerError)
        => new() { Result = data, IsSuccessful = false, Message = message, StatusCode = code };
    
    public static ServiceResponse<T> Success<T>(T data, string message = "", HttpStatusCode code = HttpStatusCode.OK)
        => new() { Result = data, IsSuccessful = true, Message = message, StatusCode = code };
}
