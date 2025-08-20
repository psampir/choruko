using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Task;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Household;
using ChorukoAPI.Mappers;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            var result = await householdTaskService.CreateHouseholdTaskAsync(householdId, request);

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
        public async Task<ActionResult<ApiResponse<List<HouseholdResponse>>>> GetAllHouseholdTaskAsync()
        {
            var result = await householdTaskService.GetAllHouseholdTaskAsync();

            return result.ToApiResponse();
        }

        [HttpGet("{householdId}/{taskId}")]
        [Authorize(Roles = "HouseholdAdmin,Participant")]
        public async Task<ActionResult<ApiResponse<HouseholdResponse>>> GetHouseholdTaskByIdAsync([FromRoute] string activityId)
        {
            var result = await householdTaskService.GetHouseholdTaskByIdAsync(activityId);

            return result.ToApiResponse();
        }
    }
}
