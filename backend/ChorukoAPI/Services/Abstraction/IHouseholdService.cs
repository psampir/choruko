namespace ChorukoAPI.Services.Abstraction;

using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Requests.Household;
using ChorukoAPI.Contracts.Responses.Household;

public interface IHouseholdService
{
    Task<ServiceResponse<OperationResultDto>> CreateHouseholdAsync(CreateHouseholdRequest request, string userId);
    
    Task<ServiceResponse<OperationResultDto>> DeleteUserFromHouseholdAsync(string userIdToDelete, string currentUserId);
    Task<ServiceResponse<OperationResultDto>> AddUserToHouseholdAsync(string userIdToAdd, string currentUserId);
}