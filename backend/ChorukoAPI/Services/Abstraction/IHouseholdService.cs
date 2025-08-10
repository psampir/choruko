namespace ChorukoAPI.Services.Abstraction;

using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Requests.Household;
using ChorukoAPI.Contracts.Responses.Household;

public interface IHouseholdService
{
    Task<ServiceResponse<HouseholdResponse>> CreateHouseholdAsync(CreateHouseholdRequest request, string userId);
    
    //Task<ServiceResponse<HouseholdResponse>> GetHouseholdDetailsAsync(string householdId);
}