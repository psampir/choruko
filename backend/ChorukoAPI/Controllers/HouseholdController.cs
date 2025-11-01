// Controllers/HouseholdController.cs
using ChorukoAPI.Extensions;
using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Household;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Household;
using ChorukoAPI.Mappers;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChorukoAPI.Controllers;

[ApiController] 
[Route("api/[controller]")] 
public class HouseholdController(IHouseholdService _householdService) : ControllerBase
{
    [HttpGet("household-info")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<HouseholdInfoResponse>>> GetHouseholdInfo()
    {
        var result = await _householdService.GetHouseholdInfoAsync();

        return result.ToApiResponse("User info");
    }


    [HttpPost("create")] 
    [Authorize(Roles="None")]
    public async Task<ActionResult<ApiResponse<HouseholdResponse>>> CreateHousehold([FromForm] CreateHouseholdRequest request)
    {

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); 
        }
        
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<HouseholdResponse>(null, "User not authenticated.").ToApiResponse());
        }
        
        var result = await _householdService.CreateHouseholdAsync(request, userId);
        
        return result.ToCreatedApiResponse("Create household");
    }
    
    [HttpDelete("{userIdToDelete}/remove")]
    [Authorize(Roles="HouseholdAdmin")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> DeleteUserFromHousehold(string userIdToDelete)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized(ServiceResponseFactory.Error<OperationResultDto>(null, "Current user not authenticated.").ToApiResponse());
        }
        
        var result = await _householdService.DeleteUserFromHouseholdAsync(userIdToDelete, currentUserId);
        
        return result.ToApiResponse("Delete participant");
    }
    
    [HttpPost("{userIdToAdd}/add")]
    [Authorize(Roles="HouseholdAdmin")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> AddUserToHousehold(string userIdToAdd)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var result = await _householdService.AddUserToHouseholdAsync(userIdToAdd, currentUserId);

        return result.ToApiResponse("Add participant");
    }
    
    [HttpDelete("delete")]
    [Authorize(Roles="HouseholdAdmin")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> DeleteHousehold()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<OperationResultDto>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await _householdService.DeleteHouseholdAsync(userId);

        return result.ToApiResponse("Delete household");
    }
    
    [HttpPatch("rename")]
    [Authorize(Roles = "HouseholdAdmin")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> RenameHousehold([FromBody] RenameHouseholdRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<OperationResultDto>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await _householdService.RenameHouseholdAsync(userId, request.NewName);

        return result.ToApiResponse("Rename household");
    }
    
    [HttpPost("leave")]
    [Authorize(Roles = "HouseholdAdmin,Participant")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> LeaveHousehold()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<OperationResultDto>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await _householdService.LeaveHouseholdAsync(userId);

        return result.ToApiResponse("Leave Household");
    }
    
    [HttpPatch("change-picture")]
    [Authorize(Roles = "HouseholdAdmin")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> ChangeHouseholdPicture([FromForm] ChangeHouseholdPictureRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<OperationResultDto>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await _householdService.ChangeHouseholdPictureAsync(request, userId);

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

        return result.ToApiResponse("Change household picture");
    }
    
    [HttpPatch("{newAdminId}/set-admin")]
    [Authorize(Roles = "HouseholdAdmin")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> SetHouseholdAdmin(string newAdminId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<OperationResultDto>(null, "User not authenticated.").ToApiResponse());
        }
        
        var result = await _householdService.SetHouseholdAdminAsync(userId, newAdminId);

        return result.ToApiResponse("Set household admin");
    }
}