// Controllers/HouseholdController.cs
using ChorukoAPI.Extensions;
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
    [Authorize] 
    [Authorize(Policy = "NoneRolePolicy")]
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
}