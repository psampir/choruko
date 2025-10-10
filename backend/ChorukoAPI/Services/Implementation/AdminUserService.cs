using System.Net;
using ChorukoAPI.Contracts.Requests.User;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Extensions;
using ChorukoAPI.Mappers;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Identity;

namespace ChorukoAPI.Services.Implementation;

public class AdminUserService(
    UserManager<User> userManager,
    IAdminAuthService adminAuthService,
    IPasswordGenerator passwordGenerator,
    IMapper<UserPasswordResetData, AdminResetPasswordResponse> mapper) : IAdminUserService
{
    public async Task<ServiceResponse<AdminResetPasswordResponse>> ResetUserPasswordAsync(AdminResetPasswordRequest request)
    {
        // 1) Verify superuser credentials
        var credentialsValid = adminAuthService.VerifySuperuserCredentials<AdminResetPasswordResponse>(
            request.DbUsername, 
            request.DbPassword
        );
        if (!credentialsValid.IsSuccessful)
        {
            return credentialsValid;
        }

        // 2) Find the target user
        var user = await userManager.FindByEmailAsync(request.UserEmail);
        if (user == null)
        {
            return ServiceResponseFactory.Error<AdminResetPasswordResponse>(
                null,
                "User not found.",
                HttpStatusCode.NotFound
            );
        }

        // 3) Generate a password that satisfies Identity PasswordOptions
        var newPassword = passwordGenerator.GenerateCompliantPassword(userManager.Options.Password);

        // 4) Reset password using Identity
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await userManager.ResetPasswordAsync(user, token, newPassword);
        if (!resetResult.Succeeded)
        {
            var errors = string.Join("; ", resetResult.Errors.Select(e => e.Description));
            return ServiceResponseFactory.Error<AdminResetPasswordResponse>(
                null,
                $"Password reset failed: {errors}.",
                HttpStatusCode.BadRequest
            );
        }

        // 5) Refresh stamps and persist
        await userManager.UpdateSecurityStampAsync(user);
        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
            return ServiceResponseFactory.Error<AdminResetPasswordResponse>(
                null,
                $"Failed to update user: {errors}.",
                HttpStatusCode.InternalServerError
            );
        }

        // 6) Map the response
        var response = await mapper.Map(new UserPasswordResetData
        {
            User = user,
            NewPassword = newPassword
        });

        return ServiceResponseFactory.Success(
            response,
            "Password reset successfully.",
            HttpStatusCode.OK
        );
    }
}
