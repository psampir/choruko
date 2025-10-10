using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.User;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.User;
using ChorukoAPI.Data;
using ChorukoAPI.Extensions;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Threading.Tasks;

namespace ChorukoAPI.Services.Implementation;

public class UserService(UserManager<User> _userManager, IHttpContextAccessor _contextAccessor, DataContext _context, IWebHostEnvironment _webHostEnvironment) : IUserService
{
    public async Task<ServiceResponse<UserInfoResponse>> GetUserInfoAsync()
    {
        var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext?.User);

        if (user == null)
        {
            return ServiceResponseFactory.Error(new UserInfoResponse("", "", "", false, null), "User not found.");
        }

        var hasHousehold = !string.IsNullOrEmpty(user.HouseholdId);
        return ServiceResponseFactory.Success(new UserInfoResponse(
            user.FirstName, 
            user.LastName, 
            user.Email, 
            hasHousehold, 
            user.HouseholdId));
    }

    public async Task<ServiceResponse<OperationResultDto>> ChangeUserNamesAsync(ChangeUserNamesRequest request, string userId)
    {
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
        
        if (!string.IsNullOrEmpty(user.ProfilePicturePath))
        {
            DeleteProfilePicture(user.ProfilePicturePath);
        }
        
        var uploadResponse = await UploadProfilePicture(request.ProfilePicture);
        if (!uploadResponse.IsSuccessful)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, uploadResponse.Message, uploadResponse.StatusCode);
        }
        
        user.ProfilePicturePath = uploadResponse.Result;
        
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Failed to update user's profile picture path.", HttpStatusCode.InternalServerError);
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
    
    private async Task<ServiceResponse<string>> UploadProfilePicture(IFormFile? file)
    {
        
        if (file == null)
        {
            return ServiceResponseFactory.Success<string>(null, "No profile picture provided.", HttpStatusCode.OK);
        }

        try
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "users");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            string profilePicturePath = Path.Combine("uploads", "users", uniqueFileName).Replace("\\", "/");
            
            return ServiceResponseFactory.Success(profilePicturePath, "Profile picture uploaded successfully.", HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<string>(null, $"Failed to upload profile picture: {ex.Message}.", HttpStatusCode.InternalServerError);
        }
    }

    private void DeleteProfilePicture(string filePath)
    {
        string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath);
        System.IO.File.Delete(fullPath);
    }
}