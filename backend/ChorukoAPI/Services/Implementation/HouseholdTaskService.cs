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
}

