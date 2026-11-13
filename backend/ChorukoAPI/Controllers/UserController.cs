using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Contracts.Requests.User;
using ChorukoAPI.Mappers;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ChorukoAPI.Contracts;
using ChorukoAPI.Extensions;

namespace ChorukoAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController(IUserService _userService) : ControllerBase
{
    [HttpGet("user-info")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserInfoResponse>>> GetUserInfo()
    {
        var result = await _userService.GetUserInfoAsync();

        return result.ToApiResponse("UserInfo");
    }

    [HttpGet("notification-settings")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<NotificationSettingsResponse>>> GetNotificationSettings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<NotificationSettingsResponse>(null, "User ID not found in token.").ToApiResponse());
        }

        var result = await _userService.GetNotificationSettingsAsync(userId);

        return result.ToApiResponse("Notification settings");
    }
    
    [HttpPut("change-names")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> ChangeUserNames([FromBody] ChangeUserNamesRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<OperationResultDto>(null, "User ID not found in token.").ToApiResponse());
        }

        var result = await _userService.ChangeUserNamesAsync(request, userId);
        
        return result.ToApiResponse("NewName");
    }
    
    [HttpPut("change-picture")]
    [Consumes("multipart/form-data")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> ChangeUserProfilePicture([FromForm] ChangeUserProfilePictureRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<OperationResultDto>(null, "User ID not found in token.").ToApiResponse());
        }

        var result = await _userService.ChangeUserProfilePictureAsync(request, userId);
        
        if (!result.IsSuccessful && result.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            ModelState.AddModelError("ProfilePicture", result.Message);
            return ValidationProblem(ModelState);
        }
        
        if (!result.IsSuccessful && (result.StatusCode == System.Net.HttpStatusCode.RequestEntityTooLarge 
            || result.StatusCode == System.Net.HttpStatusCode.UnsupportedMediaType))
        {
            ModelState.AddModelError("ProfilePicture", result.Message);
            return ValidationProblem(ModelState);
        }
        
        return result.ToApiResponse("ProfilePicture");
    }
    
    [HttpDelete("delete-self")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> DeleteSelf()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<OperationResultDto>(null, "User ID not found in token.").ToApiResponse());
        }
        
        var result = await _userService.DeleteSelfAsync(userId);
        
        return result.ToApiResponse("Delete user");
    }
    
    [HttpPut("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<OperationResultDto>(null, "User ID not found in token.").ToApiResponse());
        }

        var result = await _userService.ChangePasswordAsync(request, userId);
        
        if (!result.IsSuccessful && result.StatusCode == System.Net.HttpStatusCode.BadRequest 
            && result.Result?.Metadata?.ContainsKey("IdentityResult") == true)
        {
            var identityResult = result.Result.Metadata["IdentityResult"] as IdentityResult;
            if (identityResult != null && !identityResult.Succeeded)
            {
                foreach (var error in identityResult.Errors)
                {
                    ModelState.AddModelError("NewPassword", error.Description);
                }
                return ValidationProblem(ModelState);
            }
        }
        
        return result.ToApiResponse("NewPassword");
    }
}