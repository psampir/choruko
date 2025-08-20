using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Household;
using ChorukoAPI.Contracts.Requests.Task;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Household;
using ChorukoAPI.Services.Abstraction;

namespace ChorukoAPI.Services.Implementation;

public class HouseholdTaskService : IHouseholdTaskService
{
    public Task<ServiceResponse<OperationResultDto>> CreateHouseholdTaskAsync(string householdId, CreateHouseholdTaskRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<OperationResultDto>> EditHouseholdTaskAsync(string taskId, EditHouseholdTaskRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<List<HouseholdResponse>>> GetAllHouseholdTaskAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<HouseholdResponse>> GetHouseholdTaskByIdAsync(string activityId)
    {
        throw new NotImplementedException();
    }

    private async Task<ServiceResponse<OperationResultDto>> ValidateHouseholdTaskData(HouseholdTask householdTask)
    {
        var isActivityExist = _context.Activities.Any(a => a.HouseholdId == householdTask.HouseholdId && a.Name == householdTask.Name);

        if (isActivityExist)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Activity with this name already exist", HttpStatusCode.BadRequest);
        }

        return ServiceResponseFactory.Success<OperationResultDto>(null, "Activity is valid for household");
    }
}

