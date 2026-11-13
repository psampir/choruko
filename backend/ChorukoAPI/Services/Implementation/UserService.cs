using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.User;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Data;
using ChorukoAPI.Extensions;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Threading.Tasks;
using ChorukoAPI.Mappers;

namespace ChorukoAPI.Services.Implementation;

public class UserService(
    UserManager<User> _userManager, 
    IHttpContextAccessor _contextAccessor, 
    DataContext _context, 
    IImageProcessor _imageProcessor,
    IMapper<User, UserInfoResponse> _mapper) : IUserService
{

    public async Task<ServiceResponse<UserInfoResponse>> GetUserInfoAsync()
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext?.User == null)
        {
            return ServiceResponseFactory.Error(new UserInfoResponse("", "", "", "", null, null, 0, 0, 0), "User not found.");
        }

        var user = await _userManager.GetUserAsync(httpContext.User);

        if (user == null)
        {
            return ServiceResponseFactory.Error(new UserInfoResponse("", "", "", "", null, null, 0, 0, 0), "User not found.");
        }

        return ServiceResponseFactory.Success(await _mapper.Map(user));
    }
    public async Task<ServiceResponse<OperationResultDto>> ChangeUserNamesAsync(ChangeUserNamesRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return ServiceResponseFactory.Error<OperationResultDto>(
                null, 
                "First name and last name required.", 
                HttpStatusCode.BadRequest
            );
        }
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User not found.", HttpStatusCode.NotFound);
        }
        
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Failed to update user's name.", HttpStatusCode.InternalServerError);
        }

        var response = new OperationResultDto
        {
            Operation = "ChangeUserNames",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "UserId", user.Id },
                { "NewFirstName", user.FirstName },
                { "NewLastName", user.LastName }
            }
        };

        return ServiceResponseFactory.Success(
            response,
            "User's name was successfully changed.",
            HttpStatusCode.OK
        );
    }
    public async Task<ServiceResponse<OperationResultDto>> ChangeUserProfilePictureAsync(ChangeUserProfilePictureRequest request, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User not found.", HttpStatusCode.NotFound);
        }
        
        var oldProfilePicturePath = user.ProfilePicturePath;
        
        var uploadResponse = await _imageProcessor.ProcessAsync(request.ProfilePicture, "users");

        if (!uploadResponse.IsSuccessful)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, uploadResponse.Message, uploadResponse.StatusCode);
        }
        
        string newProfilePicturePath = uploadResponse.Result!;
        
        user.ProfilePicturePath = newProfilePicturePath;
        
        try
        {
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                try
                {
                    _imageProcessor.Delete(newProfilePicturePath);
                }
                catch (Exception cleanupEx) 
                {
                    Console.WriteLine($"LOG: Failed to clean up new file after Identity update failed. Path: {newProfilePicturePath}. Error: {cleanupEx.Message}");
                }
                
                return ServiceResponseFactory.Error<OperationResultDto>(null, "Failed to update user's profile picture path in the database.", HttpStatusCode.InternalServerError);
            }
            
            if (!string.IsNullOrEmpty(oldProfilePicturePath))
            {
                try
                {
                    _imageProcessor.Delete(oldProfilePicturePath);
                }
                catch (Exception cleanupEx)
                {
                    Console.WriteLine($"LOG: Failed to clean up old file after successful DB update. Path: {oldProfilePicturePath}. Error: {cleanupEx.Message}");
                }
            }

            var response = new OperationResultDto
            {
                Operation = "ChangeUserProfilePicture",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "UserId", user.Id },
                    { "NewProfilePicturePath", user.ProfilePicturePath }
                }
            };

            return ServiceResponseFactory.Success(
                response,
                "User profile picture was successfully changed.",
                HttpStatusCode.OK
            );
        }
        catch (Exception dbEx)
        {
            _imageProcessor.Delete(newProfilePicturePath); 
            return ServiceResponseFactory.Error<OperationResultDto>(
                null, 
                $"An unexpected error occurred during database update: {dbEx.Message}", 
                HttpStatusCode.InternalServerError
            );
        }
    }
    public async Task<ServiceResponse<OperationResultDto>> DeleteSelfAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User not found.", HttpStatusCode.NotFound);
        }
        
        var userRoles = await _userManager.GetRolesAsync(user);
        
        if (userRoles.Contains("HouseholdAdmin") && user.HouseholdId != null)
        {
            var household = await _context.Households
                .Include(h => h.Participants)
                .FirstOrDefaultAsync(h => h.Id == user.HouseholdId);

            if (household != null)
            {
                int adminCount = 0;
                // Count admins by checking roles of all current participants
                foreach (var participant in household.Participants)
                {
                    var roles = await _userManager.GetRolesAsync(participant);
                    if (roles.Contains("HouseholdAdmin"))
                    {
                        adminCount++;
                    }
                }
                
                if (adminCount <= 1)
                {
                    return ServiceResponseFactory.Error<OperationResultDto>(
                        null, 
                        "Please transfer household ownership or delete it first.", 
                        HttpStatusCode.Forbidden
                    );
                }
            }
        }
        
        if (!string.IsNullOrEmpty(user.ProfilePicturePath))
        {
            _imageProcessor.Delete(user.ProfilePicturePath);
        }
        
        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Failed to delete user account.", HttpStatusCode.InternalServerError);
        }

        var response = new OperationResultDto
        {
            Operation = "DeleteSelf",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "UserId", userId }
            }
        };

        return ServiceResponseFactory.Success(
            response,
            "User account was successfully deleted.",
            HttpStatusCode.OK
        );
    }
    
    public async Task<ServiceResponse<OperationResultDto>> ChangePasswordAsync(ChangePasswordRequest request, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User not found.", HttpStatusCode.NotFound);
        }
        
        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
        {
            var errors = removeResult.Errors.Select(e => e.Description);
            var errorMessage = string.Join(" ", errors);

            return ServiceResponseFactory.Error<OperationResultDto>(
                null, 
                $"Failed to remove old password: {errorMessage}", 
                HttpStatusCode.InternalServerError
            );
        }
        
        var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword);

        if (!addResult.Succeeded)
        {
            var errorDto = new OperationResultDto
            {
                Operation = "ChangePassword",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "IdentityResult", addResult }
                }
            };
            
            return ServiceResponseFactory.Error(
                errorDto, 
                "Password validation failed.", 
                HttpStatusCode.BadRequest
            );
        }
        
        await _userManager.UpdateSecurityStampAsync(user);

        var response = new OperationResultDto
        {
            Operation = "ChangePassword",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "UserId", user.Id }
            }
        };

        return ServiceResponseFactory.Success(
            response,
            "User password was successfully changed.",
            HttpStatusCode.OK
        );
    }

}