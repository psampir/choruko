using System.Net;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Extensions;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Identity;

namespace ChorukoAPI.Services.Implementation;

public class UserPasswordResetService(
    UserManager<User> userManager,
    IPasswordGenerator passwordGenerator) : IUserPasswordResetService
{
    public async Task<ServiceResponse<UserPasswordResetData>> ResetPasswordAsync(string email)
    {
        // 1) Find the target user
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return ServiceResponseFactory.Error<UserPasswordResetData>(
                null!,
                "User not found.",
                HttpStatusCode.NotFound
            );
        }

        // 2) Generate a password that satisfies Identity PasswordOptions
        var newPassword = passwordGenerator.GenerateCompliantPassword(userManager.Options.Password);

        // 3) Reset password using Identity
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await userManager.ResetPasswordAsync(user, token, newPassword);
        if (!resetResult.Succeeded)
        {
            var errors = string.Join("; ", resetResult.Errors.Select(e => e.Description));
            return ServiceResponseFactory.Error<UserPasswordResetData>(
                null!,
                $"Password reset failed: {errors}.",
                HttpStatusCode.BadRequest
            );
        }

        // 4) Refresh stamps and persist
        await userManager.UpdateSecurityStampAsync(user);
        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
            return ServiceResponseFactory.Error<UserPasswordResetData>(
                null!,
                $"Failed to update user: {errors}.",
                HttpStatusCode.InternalServerError
            );
        }

        return ServiceResponseFactory.Success(
            new UserPasswordResetData
            {
                User = user,
                NewPassword = newPassword
            },
            "Password reset successfully.",
            HttpStatusCode.OK
        );
    }
}
