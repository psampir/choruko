using ChorukoAPI.Contracts.Responses.Invitation;
using ChorukoAPI.Models;

namespace ChorukoAPI.Mappers;

public class InvitationMapper : IMapper<Invitation, InvitationResponse>
{
    public async Task<InvitationResponse> Map(Invitation source)
    {
        return new InvitationResponse
        {
            Id = source.Id,
            ReceiverId = source.ReceiverId,
            HouseholdName = source.Household.Name,
            HouseholdImagePath = source.Household.ProfilePicturePath
        };
    }
}
