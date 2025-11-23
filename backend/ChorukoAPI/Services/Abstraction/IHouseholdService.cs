namespace ChorukoAPI.Services.Abstraction;

using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Requests.Household;
using ChorukoAPI.Contracts.Responses.Household;

public interface IHouseholdService
{
    Task<ServiceResponse<HouseholdInfoResponse>> GetHouseholdInfoAsync();
    Task<ServiceResponse<OperationResultDto>> CreateHouseholdAsync(CreateHouseholdRequest request, string userId);
    
    Task<ServiceResponse<OperationResultDto>> DeleteUserFromHouseholdAsync(string userIdToDelete, string currentUserId);
    Task<ServiceResponse<OperationResultDto>> AddUserToHouseholdAsync(string userIdToAdd, string currentUserId);
    Task<ServiceResponse<OperationResultDto>> DeleteHouseholdAsync(string userId);
    Task<ServiceResponse<OperationResultDto>> RenameHouseholdAsync(string userId, string newName);
    Task<ServiceResponse<OperationResultDto>> LeaveHouseholdAsync(string userId);
    Task<ServiceResponse<OperationResultDto>> ChangeHouseholdPictureAsync(ChangeHouseholdPictureRequest request, string userId);
    Task<ServiceResponse<OperationResultDto>> SetHouseholdAdminAsync(string userId, string newAdminId);
    Task<ServiceResponse<OperationResultDto>> ChangeHouseholdCurrencyAsync(ChangeCurrencyRequest request, string userId);
}