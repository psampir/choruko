using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Contracts.Requests.User;
using ChorukoAPI.Mappers;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
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
    [HttpGet("userInfo")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserInfoResponse>>> GetUserInfo()
    {
        var result = await _userService.GetUserInfoAsync();

        return result.ToApiResponse();
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
            return Unauthorized(ServiceResponseFactory.Error<OperationResultDto>(null, "User ID not found in token."));
        }

        var result = await _userService.ChangeUserNamesAsync(request, userId);
        
        return result.ToApiResponse();
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
            return Unauthorized(ServiceResponseFactory.Error<OperationResultDto>(null, "User ID not found in token."));
        }

        var result = await _userService.ChangeUserProfilePictureAsync(request, userId);
        
        return result.ToApiResponse();
    }
}