using ChorukoAPI.Contracts.Responses.User;

namespace ChorukoAPI.Mappers;

public class AdminResetPasswordMapper : IMapper<UserPasswordResetData, AdminResetPasswordResponse>
{
    public Task<AdminResetPasswordResponse> Map(UserPasswordResetData source)
    {
        var response = new AdminResetPasswordResponse
        {
            UserId = source.User.Id,
            UserEmail = source.User.Email ?? string.Empty,
            UserNewPassword = source.NewPassword
        };

        return Task.FromResult(response);
    }
}
