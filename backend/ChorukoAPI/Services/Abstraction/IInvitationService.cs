using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts;

namespace ChorukoAPI.Services.Abstraction;

public interface IInvitationService
{
    Task<ServiceResponse<OperationResultDto>> CreateHouseholdInvitation(string userId, string householdId, string userIdToAdd);
}
