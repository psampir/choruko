using ChorukoAPI.Contracts.Responses.Settlement;
using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Models;

namespace ChorukoAPI.Mappers;

public class SettlementMapper(IMapper<User, UserInfoResponse> userMapper) : IMapper<Settlement, SettlementResponse>
{
    public async Task<SettlementResponse> Map(Settlement source)
    {
        return new SettlementResponse
        {
            Id = source.Id,
            HoudeholsId = source.HouseholdId,
            CreatorId = source.CreatorId,
            Creator = await userMapper.Map(source.Creator),
            DateOfCreation = source.DateOfCreation,
            Description = source.Description,
            Amount = source.Amount,
            Status = source.Status,
            SettledUsers = (await Task.WhenAll(source.SettledUsers.Select(x => userMapper.Map(x)))).ToList(),
            UnsettledUsers = (await Task.WhenAll(source.UnsettledUsers.Select(x => userMapper.Map(x)))).ToList()
        };
    }
}
