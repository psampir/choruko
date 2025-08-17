using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Task;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Household;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChorukoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        [HttpPost("create/{householdId}")]
        public async Task<ActionResult<ApiResponse<OperationResultDto>>> CreateHouseholdTaskAsync([FromRoute] string householdId, CreateHouseholdTaskRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPut("edit/{householdId}/{taskId}")]
        public async Task<ActionResult<ApiResponse<OperationResultDto>>> EditHouseholdTaskAsync([FromRoute] string taskId, EditHouseholdTaskRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpGet("{householdId}")]
        public async Task<ActionResult<ApiResponse<List<HouseholdResponse>>>> GetAllHouseholdTaskAsync()
        {
            throw new NotImplementedException();
        }

        [HttpGet("{householdId}/{taskId}")]
        public async Task<ActionResult<ApiResponse<HouseholdResponse>>> GetHouseholdTaskByIdAsync([FromRoute] string activityId)
        {
            throw new NotImplementedException();
        }
    }
}
