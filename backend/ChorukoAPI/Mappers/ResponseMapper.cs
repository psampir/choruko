using ChorukoAPI.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Identity;

namespace ChorukoAPI.Mappers;

public static class ResponseMapper
{
    public static ObjectResult ToApiResponse<T>(this ServiceResponse<T> serviceResponse, string? validationKey = null) where T : class?
    {
        if (serviceResponse.IsSuccessful)
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

        return ResponseError(serviceResponse, validationKey);
    }

    public static ObjectResult ToCreatedApiResponse<T>(this ServiceResponse<T> serviceResponse, string? validationKey = null)
        where T : class?
    {
        if (serviceResponse.IsSuccessful)
        {
            return new CreatedResult(string.Empty, new ApiResponse<T>
            {
                Result = serviceResponse.Result,
                Message = serviceResponse.Message,
                IsSuccessful = serviceResponse.IsSuccessful
            });
        }

        return ResponseError(serviceResponse, validationKey);
    }

    private static ObjectResult ResponseError<T>(ServiceResponse<T> serviceResponse, string? validationKey) where T : class?
    {
        var modelState = new ModelStateDictionary();
        var statusCode = (int)serviceResponse.StatusCode;
        var validationProblemDetails = new ValidationProblemDetails(modelState);

        if (serviceResponse.Result is IdentityResult identityResult && !identityResult.Succeeded)
        {
            string key = validationKey ?? string.Empty;
            foreach (var error in identityResult.Errors)
            {
                modelState.AddModelError(key, error.Description);
            }

            validationProblemDetails = new ValidationProblemDetails(modelState)
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "One or more validation errors occurred."
            };
            return new ObjectResult(validationProblemDetails) { StatusCode = validationProblemDetails.Status };
        }

        if (serviceResponse.Result is Microsoft.AspNetCore.Identity.SignInResult signInResult && !signInResult.Succeeded)
        {
            string key = validationKey ?? string.Empty;
            modelState.AddModelError(key, serviceResponse.Message ?? "Invalid login attempt.");

            validationProblemDetails = new ValidationProblemDetails(modelState)
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "One or more validation errors occurred."
            };
            return new ObjectResult(validationProblemDetails) { StatusCode = validationProblemDetails.Status };
        }

        modelState.AddModelError(validationKey, serviceResponse.Message ?? "An error occurred.");

        validationProblemDetails = new ValidationProblemDetails(modelState)
        {
            Status = statusCode,
            Title = "One or more validation errors occurred."
        };
        return new ObjectResult(validationProblemDetails) { StatusCode = validationProblemDetails.Status };
    }
}