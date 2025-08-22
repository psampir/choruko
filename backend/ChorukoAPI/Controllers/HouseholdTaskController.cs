using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Task;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Household;
using ChorukoAPI.Contracts.Responses.Task;
using ChorukoAPI.Extensions;
using ChorukoAPI.Mappers;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChorukoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HouseholdTaskController(IHouseholdTaskService householdTaskService) : ControllerBase
    {
        [HttpPost("create/{householdId}")]
        [Authorize(Roles = "HouseholdAdmin,Participant")]
        public async Task<ActionResult<ApiResponse<OperationResultDto>>> CreateHouseholdTaskAsync([FromRoute] string householdId, CreateHouseholdTaskRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ServiceResponseFactory.Error<HouseholdResponse>(null, "User not authenticated.").ToApiResponse());
            }

            var result = await householdTaskService.CreateHouseholdTaskAsync(householdId, request, userId);

            return result.ToCreatedApiResponse();
        }

        [HttpPut("edit/{householdId}/{taskId}")]
        [Authorize(Roles = "HouseholdAdmin,Participant")]
        public async Task<ActionResult<ApiResponse<OperationResultDto>>> EditHouseholdTaskAsync([FromRoute] string taskId, EditHouseholdTaskRequest request)
        {
            var result = await householdTaskService.EditHouseholdTaskAsync(taskId, request);

            return result.ToCreatedApiResponse();
        }

        [HttpGet("{householdId}")]
        [Authorize(Roles = "HouseholdAdmin,Participant")]
        public async Task<ActionResult<ApiResponse<List<HouseholdTaskResponse>>>> GetAllHouseholdTaskAsync([FromRoute] string householdId)
        {
            var result = await householdTaskService.GetAllHouseholdTaskAsync(householdId);

            return result.ToApiResponse();
        }

        [HttpGet("{householdId}/{taskId}")]
        [Authorize(Roles = "HouseholdAdmin,Participant")]
        public async Task<ActionResult<ApiResponse<HouseholdTaskResponse>>> GetHouseholdTaskByIdAsync([FromRoute] string householdId)
        {
            var result = await householdTaskService.GetHouseholdTaskByIdAsync(householdId);

            return result.ToApiResponse();
        }
    }
}
