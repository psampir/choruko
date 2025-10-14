using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Models;

namespace ChorukoAPI.Mappers;

public class UserInfoMapper : IMapper<User, UserInfoResponse>
{
    public async Task<UserInfoResponse> Map(User source)
    {
        return new UserInfoResponse(source.Id, source.FirstName, source.LastName, source.Email, source.HouseholdId, source.ProfilePicturePath);
    }
}
