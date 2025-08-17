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
    [HttpPost("create")]
    [Authorize(Roles = "HouseholdAdmin,Participant")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> CreateActivity([FromBody] CreateActivityRequest createActivityRequest)
    {
        var result = await _activityService.CreateActivityAsync(createActivityRequest);
        return result.ToCreatedApiResponse();
    }

    [HttpPut("edit/{activityId}")]
    [Authorize(Roles = "HouseholdAdmin,Participant")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> EditActivity([FromRoute] string activityId, [FromBody] EditActivityRequest updateActivityRequest)
    {
        var result = await _activityService.EditActivityAsync(activityId, updateActivityRequest);
        return result.ToCreatedApiResponse();
    }

    [HttpDelete("delete/{activityId}")]
    [Authorize(Roles = "HouseholdAdmin,Participant")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> DeleteActivity([FromRoute] string activityId)
    {
        var result = await _activityService.DeleteActivityAsync(activityId);
        return result.ToCreatedApiResponse();
    }

    [HttpGet("all/{householdId}")]
    [Authorize(Roles = "HouseholdAdmin,Participant")]
    public async Task<ActionResult<ApiResponse<List<ActivityResponse>>>> GetActivities([FromRoute] string householdId)
    {
        var result = await _activityService.GetActivitiesAsync(householdId);
        return result.ToApiResponse();
    }

    [HttpGet("byId/{activityId}")]
    [Authorize(Roles = "HouseholdAdmin,Participant")]
    public async Task<ActionResult<ApiResponse<ActivityResponse>>> GetActivity([FromRoute] string activityId)
    {
        var result = await _activityService.GetActivityByIdAsync(activityId);
        return result.ToApiResponse();
    }
}

