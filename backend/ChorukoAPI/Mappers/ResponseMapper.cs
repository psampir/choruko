using ChorukoAPI.Contracts.DTOs;
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
            ErrorMessage = serviceResponse.ErrorMessage
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
                ErrorMessage = serviceResponse.ErrorMessage
            });
        }

        return new ObjectResult(new ApiResponse<T>
        {
            Result = serviceResponse.Result,
            ErrorMessage = serviceResponse.ErrorMessage
        })
        {
            StatusCode = (int)serviceResponse.StatusCode
        };
    }
}