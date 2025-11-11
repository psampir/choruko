using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Responses.Invitation;

namespace ChorukoAPI.Services.Abstraction;

public interface IInvitationService
{
    Task<ServiceResponse<OperationResultDto>> CreateHouseholdInvitation(string userId, string householdId, string userIdToAdd);
    Task<ServiceResponse<List<InvitationResponse>>> GetAllInvitations(string userId);
}
