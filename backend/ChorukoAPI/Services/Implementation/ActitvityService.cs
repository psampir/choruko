using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Activity;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Activity;
using ChorukoAPI.Services.Abstraction;

namespace ChorukoAPI.Services.Implementation
{
    public class ActitvityService : IActivityService
    {
        public Task<ServiceResponse<OperationResultDto>> CreateActivityAsync(CreateActivityRequest createActivityDto)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<OperationResultDto>> DeleteActivityAsync(int activityId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<List<ActivityResponse>>> GetActivitiesAsync(string? householdId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<ActivityResponse>> GetActivityByIdAsync(string activityId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<OperationResultDto>> UpdateActivityAsync(int activityId, UpdateActivityRequest updateActivityRequest)
        {
            throw new NotImplementedException();
        }
    }
}
