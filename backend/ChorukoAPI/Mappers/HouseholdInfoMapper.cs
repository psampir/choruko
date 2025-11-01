using ChorukoAPI.Contracts.Responses.Household;
using ChorukoAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ChorukoAPI.Mappers;

public class HouseholdInfoMapper : IMapper<Household, HouseholdInfoResponse>
{
    public async Task<HouseholdInfoResponse> Map(Household source)
    {
        var members = source.Participants?
            .Select(p => new HouseholdMemberResponse(
                p.Id,
                p.FirstName,
                p.LastName,
                p.ProfilePicturePath,
                p.PointsCurrentMonth,
                p.PointsCurrentYear,
                p.PointsAllTime
            ))
            .ToList() ?? new List<HouseholdMemberResponse>();

        return new HouseholdInfoResponse(
            source.Id,
            source.Name,
            source.AdminId,
            source.Admin.FirstName,
            source.Admin.LastName,
            source.Participants?.Count ?? 0,
            source.ProfilePicturePath,
            members);
    }
}
