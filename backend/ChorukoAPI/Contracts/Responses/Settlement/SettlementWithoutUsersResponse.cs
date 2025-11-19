using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Models;

namespace ChorukoAPI.Contracts.Responses.Settlement;

public class SettlementWithoutUsersResponse
{
    public required string Id { get; set; }

    public required string HouseholdId { get; set; }

    public required string CreatorId { get; set; }

    public required UserInfoResponse Creator { get; set; }

    public DateTime DateOfCreation { get; set; }

    public required string Description { get; set; }

    public decimal Amount { get; set; }

    public SettlementStatus Status { get; set; }
}
