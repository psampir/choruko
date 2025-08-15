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
    [HttpPost]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> CreateActivity([FromBody] CreateActivityRequest createActivityRequest)
    {
        var result = await _activityService.CreateActivityAsync(createActivityRequest);
        return result.ToCreatedApiResponse();
    }
}

