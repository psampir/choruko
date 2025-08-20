using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Task;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Household;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChorukoAPI.Services.Abstraction;

public interface IHouseholdTaskService
{
    Task<ServiceResponse<OperationResultDto>> CreateHouseholdTaskAsync(string householdId, CreateHouseholdTaskRequest request, string userId);
    Task<ServiceResponse<OperationResultDto>> EditHouseholdTaskAsync(string taskId, EditHouseholdTaskRequest request);
    Task<ServiceResponse<List<HouseholdResponse>>> GetAllHouseholdTaskAsync();
    Task<ServiceResponse<HouseholdResponse>> GetHouseholdTaskByIdAsync(string activityId);
}
