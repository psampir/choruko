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

        [HttpPut("edit/{taskId}")]
        [Authorize(Roles = "HouseholdAdmin,Participant")]
        public async Task<ActionResult<ApiResponse<OperationResultDto>>> EditHouseholdTaskAsync([FromRoute] string taskId, EditHouseholdTaskRequest request)
        {
            var result = await householdTaskService.EditHouseholdTaskAsync(taskId, request);

            return result.ToCreatedApiResponse();
        }

        [HttpGet("all/{householdId}")]
        [Authorize(Roles = "HouseholdAdmin,Participant")]
        public async Task<ActionResult<ApiResponse<List<HouseholdTaskResponse>>>> GetAllHouseholdTaskAsync([FromRoute] string householdId)
        {
            var result = await householdTaskService.GetAllHouseholdTaskAsync(householdId);

            return result.ToApiResponse();
        }

        [HttpGet("{taskId}")]
        [Authorize(Roles = "HouseholdAdmin,Participant")]
        public async Task<ActionResult<ApiResponse<HouseholdTaskResponse>>> GetHouseholdTaskByIdAsync([FromRoute] string taskId)
        {
            var result = await householdTaskService.GetHouseholdTaskByIdAsync(taskId);

            return result.ToApiResponse();
        }

        [HttpPut("assign/{taskId}")]
        [Authorize(Roles = "HouseholdAdmin,Participant")]
        public async Task<ActionResult<ApiResponse<OperationResultDto>>> AssignToTask( [FromRoute] string taskId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ServiceResponseFactory.Error<HouseholdResponse>(null, "User not authenticated.").ToApiResponse());
            }

            var result = await householdTaskService.AssignUserToTask(taskId, userId);

            return result.ToApiResponse();
        }

        [HttpPut("unassign/{taskId}")]
        [Authorize(Roles = "HouseholdAdmin,Participant")]
        public async Task<ActionResult<ApiResponse<OperationResultDto>>> UnAssignFromTask([FromRoute] string taskId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ServiceResponseFactory.Error<HouseholdResponse>(null, "User not authenticated.").ToApiResponse());
            }

            var result = await householdTaskService.UnAssignUserFromTask(taskId, userId);

            return result.ToApiResponse();
        }

        [HttpPut("completed/{taskId}")]
        [Authorize(Roles = "HouseholdAdmin,Participant")]
        public async Task<ActionResult<ApiResponse<OperationResultDto>>> MarkTaskAsCompleted([FromRoute] string taskId)
        {
            var result = await householdTaskService.MarkTaskAsCompleted(taskId);

            return result.ToApiResponse();
        }

        [HttpPut("uncompleted/{taskId}")]
        [Authorize(Roles = "HouseholdAdmin,Participant")]
        public async Task<ActionResult<ApiResponse<OperationResultDto>>> UnMarkTaskAsCompleted([FromRoute] string taskId)
        {
            var result = await householdTaskService.UnMarkTaskAsCompleted(taskId);

            return result.ToApiResponse();
        }

        [HttpDelete("delete/{taskId}")]
        [Authorize(Roles = "HouseholdAdmin,Participant")]
        public async Task<ActionResult<ApiResponse<OperationResultDto>>> DeleteTask([FromRoute] string taskId)
        {
            var result = await householdTaskService.DeleteHouseholdTask(taskId);

            return result.ToApiResponse();
        }
    }
}
