using ChorukoAPI.Contracts.Responses.Settlement;
using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Models;

namespace ChorukoAPI.Mappers;

public class SettlementWithoutUsersMapper(IMapper<User, UserInfoResponse> userMapper) : IMapper<Settlement, SettlementWithoutUsersResponse>
{
    public async Task<SettlementWithoutUsersResponse> Map(Settlement source)
    {
        return new SettlementWithoutUsersResponse
        {
            Id = source.Id,
            HouseholdId = source.HouseholdId,
            CreatorId = source.CreatorId,
            Creator = await userMapper.Map(source.Creator),
            DateOfCreation = source.DateOfCreation,
            Description = source.Description,
            Amount = source.Amount,
            Status = source.Status
        };
    }
}
