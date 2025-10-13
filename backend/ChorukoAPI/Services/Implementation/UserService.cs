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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using Microsoft.Extensions.Options;
using ChorukoAPI.Mappers;

namespace ChorukoAPI.Services.Implementation;

public class UserService(
    UserManager<User> _userManager, 
    IHttpContextAccessor _contextAccessor, 
    DataContext _context, 
    IWebHostEnvironment _webHostEnvironment,
    IOptions<ImageSettings> _options,
    IMapper<User, UserInfoResponse> _mapper) : IUserService
{

    public async Task<ServiceResponse<UserInfoResponse>> GetUserInfoAsync()
    {
        var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext?.User);

        if (user == null)
        {
            return ServiceResponseFactory.Error(new UserInfoResponse("", "", "", "", null, null), "User not found.");
        }

        return ServiceResponseFactory.Success(await _mapper.Map(user));
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
    private async Task<ServiceResponse<string>> ValidateAndProcessImage(IFormFile file)
    {
        if (file.Length > _options.Value.MaxFileSize)
        {
            return ServiceResponseFactory.Error<string>(null, $"File size must not exceed {_options.Value.MaxFileSize / 1024 / 1024}MB.", HttpStatusCode.RequestEntityTooLarge);
        }
        
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_options.Value.AllowedFormats.Contains(fileExtension))
        {
            return ServiceResponseFactory.Error<string>(null, $"Invalid file format. Only {string.Join(", ", _options.Value.AllowedFormats)} are allowed.", HttpStatusCode.UnsupportedMediaType);
        }

        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "users"); // Note: users folder
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }
        
        string uniqueFileName = Guid.NewGuid().ToString() + ".jpg";
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        try
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using (Image image = await Image.LoadAsync(memoryStream))
                {
                    if (image.Width < _options.Value.MinDimension || image.Height < _options.Value.MinDimension)
                    {
                        return ServiceResponseFactory.Error<string>(null, $"Image dimensions are too small. Minimum required: {_options.Value.MinDimension}x{_options.Value.MinDimension} pixels.", HttpStatusCode.BadRequest);
                    }
                    
                    if (image.Width > _options.Value.MaxDimension || image.Height > _options.Value.MaxDimension)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(_options.Value.MaxDimension, _options.Value.MaxDimension),
                            Mode = ResizeMode.Crop
                        }));
                    }
                    
                    await image.SaveAsJpegAsync(filePath, new JpegEncoder { Quality = 100 });
                }
            }

            string profilePicturePath = Path.Combine("uploads", "users", uniqueFileName).Replace("\\", "/");
            return ServiceResponseFactory.Success(profilePicturePath, "Profile picture processed successfully.", HttpStatusCode.OK);
        }
        catch (ImageFormatException)
        {
            return ServiceResponseFactory.Error<string>(null, "The file is not a valid or recognizable image.", HttpStatusCode.UnsupportedMediaType);
        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<string>(null, $"Failed to process image: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
    private async Task<ServiceResponse<string>> UploadProfilePicture(IFormFile? file)
    {
        // If no file is provided, it's not an error.
        if (file == null)
        {
            return ServiceResponseFactory.Success<string>(null, "No profile picture provided.", HttpStatusCode.OK);
        }

        // Delegate all validation, processing, and saving to the new method
        var processingResponse = await ValidateAndProcessImage(file);

        if (!processingResponse.IsSuccessful)
        {
            // Return the error generated by the validation/processing method
            return processingResponse;
        }

        // Return success with the file path
        return ServiceResponseFactory.Success(
            processingResponse.Result!,
            "Profile picture uploaded and processed successfully.",
            HttpStatusCode.OK
        );
    }
    private void DeleteProfilePicture(string filePath)
    {
        string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath);
        System.IO.File.Delete(fullPath);
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
                        "Cannot delete account. You are the sole administrator of your household. Please transfer ownership or delete the household first.", 
                        HttpStatusCode.Forbidden
                    );
                }
            }
        }
        
        if (!string.IsNullOrEmpty(user.ProfilePicturePath))
        {
            DeleteProfilePicture(user.ProfilePicturePath);
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

}