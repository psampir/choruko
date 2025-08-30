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
        
        return result.ToCreatedApiResponse();
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
        
        return result.ToApiResponse();
    }
    
    [HttpPost("{userIdToAdd}/add")]
    [Authorize(Roles="HouseholdAdmin")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> AddUserToHousehold(string userIdToAdd)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var result = await _householdService.AddUserToHouseholdAsync(userIdToAdd, currentUserId);

        return result.ToApiResponse();
    }
}