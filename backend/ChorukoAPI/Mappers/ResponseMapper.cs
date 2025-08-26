using ChorukoAPI.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ChorukoAPI.Mappers;

public static class ResponseMapper
{
    public static ObjectResult ToApiResponse<T>(this ServiceResponse<T> serviceResponse) where T : class?
    {
        return new ObjectResult(new ApiResponse<T>
        {
            Result = serviceResponse.Result,
            Message = serviceResponse.Message,
            IsSuccessful = serviceResponse.IsSuccessful
        })
        {
            StatusCode = (int)serviceResponse.StatusCode
        };
    }

    public static ObjectResult ToCreatedApiResponse<T>(this ServiceResponse<T> serviceResponse, string route = "")
        where T : class?
    {
        if (string.IsNullOrEmpty(route))
        {
            var location = $"{route}";

            return new CreatedResult(location, new ApiResponse<T>
            {
                Result = serviceResponse.Result,
                Message = serviceResponse.Message,
                IsSuccessful = serviceResponse.IsSuccessful
            });
        }

        return new ObjectResult(new ApiResponse<T>
        {
            Result = serviceResponse.Result,
            Message = serviceResponse.Message,
            IsSuccessful = serviceResponse.IsSuccessful
        })
        {
            StatusCode = (int)serviceResponse.StatusCode
        };
    }
}