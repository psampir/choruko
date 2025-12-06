using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Activity;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Activity;
using ChorukoAPI.Mappers;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChorukoAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ActivityController(IActivityService _activityService) : ControllerBase
{
    [HttpPost("create/{householdId}")]
    [Authorize(Roles = "HouseholdAdmin")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> CreateActivity([FromRoute] string householdId, [FromBody] CreateActivityRequest createActivityRequest)
    {
        var result = await _activityService.CreateActivityAsync(householdId, createActivityRequest);
        return result.ToCreatedApiResponse("Create activity");
    }

    [HttpPut("edit/{householdId}/{activityId}")]
    [Authorize(Roles = "HouseholdAdmin")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> EditActivity([FromRoute] string householdId, [FromRoute] string activityId, [FromBody] EditActivityRequest updateActivityRequest)
    {
        var result = await _activityService.EditActivityAsync(activityId, updateActivityRequest);
        return result.ToCreatedApiResponse("Edit activity");
    }

    [HttpDelete("delete/{activityId}")]
    [Authorize(Roles = "HouseholdAdmin")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> DeleteActivity([FromRoute] string activityId)
    {
        var result = await _activityService.DeleteActivityAsync(activityId);
        return result.ToCreatedApiResponse("Delete activity");
    }

    [HttpGet("{householdId}")]
    [Authorize(Roles = "HouseholdAdmin")]
    public async Task<ActionResult<ApiResponse<List<ActivityResponse>>>> GetActivities([FromRoute] string householdId)
    {
        var result = await _activityService.GetActivitiesAsync(householdId);
        return result.ToApiResponse("Activity list");
    }

    [HttpGet("{householdId}/{activityId}")]
    [Authorize(Roles = "HouseholdAdmin")]
    public async Task<ActionResult<ApiResponse<ActivityResponse>>> GetActivity([FromRoute] string householdId, [FromRoute] string activityId)
    {
        var result = await _activityService.GetActivityByIdAsync(activityId);
        return result.ToApiResponse("Activity details");
    }
}

