using ChorukoAPI.Contracts.Responses.Household;
using ChorukoAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
                p.PointsCurrentMonth,
                p.PointsCurrentYear,
                p.PointsAllTime
            ))
            .ToList() ?? new List<HouseholdParticipantResponse>();

        string currencySymbol = GetCurrencySymbol(source.CurrencyCode);
        string ISOcurrencySymbol = source.CurrencyCode;
        
        return new HouseholdInfoResponse(
            source.Id,
            source.Name,
            source.AdminId,
            source.Admin.FirstName,
            source.Admin.LastName,
            source.Participants?.Count ?? 0,
            source.ProfilePicturePath,
            currencySymbol,
            ISOcurrencySymbol,
            participants);
    }
    
    private string GetCurrencySymbol(string? isoCurrencyCode)
    {
        if (string.IsNullOrEmpty(isoCurrencyCode)) return "$"; // Default fallback

        try
        {
            var region = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(c => new RegionInfo(c.Name))
                .FirstOrDefault(r => r.ISOCurrencySymbol.Equals(isoCurrencyCode, StringComparison.InvariantCultureIgnoreCase));

            return region?.CurrencySymbol ?? isoCurrencyCode;
        }
        catch
        {
            return isoCurrencyCode; // Return the code itself if mapping fails
        }
    }
}
