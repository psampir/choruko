using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Task;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Household;
using ChorukoAPI.Contracts.Responses.Task;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChorukoAPI.Services.Abstraction;

public interface IHouseholdTaskService
{
    Task<ServiceResponse<OperationResultDto>> AssignUserToTask(string taskId, string userId);
    Task<ServiceResponse<OperationResultDto>> UnAssignUserFromTask(string taskId, string userId);
    Task<ServiceResponse<OperationResultDto>> CreateHouseholdTaskAsync(string householdId, CreateHouseholdTaskRequest request, string userId);
    Task<ServiceResponse<OperationResultDto>> EditHouseholdTaskAsync(string taskId, EditHouseholdTaskRequest request);
    Task<ServiceResponse<List<HouseholdTaskResponse>>> GetAllHouseholdTaskAsync(string householdId);
    Task<ServiceResponse<HouseholdTaskResponse>> GetHouseholdTaskByIdAsync(string taskId);
    Task<ServiceResponse<OperationResultDto>> MarkTaskAsCompleted(string taskId);
    Task<ServiceResponse<OperationResultDto>> UnMarkTaskAsCompleted(string taskId);
    Task<ServiceResponse<OperationResultDto>> DeleteHouseholdTask(string taskId);
}
