using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Models;

namespace ChorukoAPI.Contracts.Responses.Settlement;

public class SettlementResponse
{
    public string Id { get; set; }

    public string HoudeholsId { get; set; }

    public string CreatorId { get; set; }

    public UserInfoResponse Creator { get; set; }

    public DateTime DateOfCreation { get; set; }

    public string Description { get; set; }

    public decimal Amount { get; set; }

    public SettlementStatus Status { get; set; }

    public List<UserInfoResponse>? SettledUsers { get; set; }
    public List<UserInfoResponse>? UnsettledUsers { get; set; }
}
