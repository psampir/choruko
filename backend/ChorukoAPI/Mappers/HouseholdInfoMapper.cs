using ChorukoAPI.Contracts.Responses.Household;
using ChorukoAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ChorukoAPI.Mappers;

public class HouseholdInfoMapper : IMapper<Household, HouseholdInfoResponse>
{
    public async Task<HouseholdInfoResponse> Map(Household source)
    {
        var participants = source.Participants?
            .Select(p => new HouseholdParticipantResponse(
                p.Id,
                p.FirstName,
                p.LastName,
                p.ProfilePicturePath,
                0,
                0,
                0
            ))
            .ToList() ?? new List<HouseholdParticipantResponse>();

        return new HouseholdInfoResponse(
            source.Id,
            source.Name,
            source.AdminId,
            source.Admin.FirstName,
            source.Admin.LastName,
            source.Participants?.Count ?? 0,
            source.ProfilePicturePath,
            participants);
    }
}
