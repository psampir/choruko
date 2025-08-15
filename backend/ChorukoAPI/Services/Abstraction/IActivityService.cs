using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Activity;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Activity;

namespace ChorukoAPI.Services.Abstraction
{
    public interface IActivityService
    {
        Task<ServiceResponse<OperationResultDto>> CreateActivityAsync(CreateActivityRequest createActivityDto);
        Task<ServiceResponse<OperationResultDto>> UpdateActivityAsync(int activityId, EditActivityRequest updateActivityRequest);
        Task<ServiceResponse<OperationResultDto>> DeleteActivityAsync(int activityId);
        Task<ServiceResponse<List<ActivityResponse>>> GetActivitiesAsync(string? householdId);
        Task<ServiceResponse<ActivityResponse>> GetActivityByIdAsync(string activityId);
    }
}
