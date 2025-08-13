using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Household;
using ChorukoAPI.Data;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Identity;
using System.Net; 
using ChorukoAPI.Extensions;
using ChorukoAPI.Contracts.Responses;


namespace ChorukoAPI.Services.Implementation;

public class HouseholdService(DataContext _context, UserManager<User> _userManager, IWebHostEnvironment _webHostEnvironment) : IHouseholdService
{
    public async Task<ServiceResponse<OperationResultDto>> CreateHouseholdAsync(CreateHouseholdRequest request, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User not found.", HttpStatusCode.NotFound);
        }
        
        var validationResponse = await ValidateUser(user);
        if (validationResponse.IsSuccessful)
        {
            return validationResponse;
        }
        
        var uploadResponse = await UploadProfilePicture(request);
        if (!uploadResponse.IsSuccessful)
        {
            // If upload failed, return the error response from the upload method
            return ServiceResponseFactory.Error<OperationResultDto>(null, uploadResponse.Message, uploadResponse.StatusCode);
        }

        string? profilePicturePath = uploadResponse.Result;

        // Create the new Household entity
        var household = new Household
        {
            Id = Guid.NewGuid().ToString(), 
            Name = request.Name,
            AdminId = user.Id, 
            Participants = new List<User>(),
            ProfilePicturePath = profilePicturePath 
        };
        
        household.Participants.Add(user);
        user.HouseholdId = household.Id;

        // Add the new household to the database context
        _context.Households.Add(household);
        var roleResponse = await MakeHouseholdAdminRole(user);
        if (!roleResponse.IsSuccessful)
        {
            // If role update fails, return the error immediately
            return roleResponse;
        }
        
        await _context.SaveChangesAsync(); 
        
        var response = new OperationResultDto
        {
            Operation = "CreateHousehold",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {

                { "HouseholdName", household.Name },
                { "ProfilePicturePath", household.ProfilePicturePath }
            }
        };

        return ServiceResponseFactory.Success(
            response,
            "Household was succesfully created",
            HttpStatusCode.OK
        );
    }

    private async Task<ServiceResponse<OperationResultDto>> ValidateUser(User user)
    {
        // Ensure the user is not already part of a household
        if (user.HouseholdId != null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User already belongs to a household.", HttpStatusCode.Conflict);
        }

        // Verify that the user has the "None" role before allowing household creation
        var userRoles = await _userManager.GetRolesAsync(user);
        if (!userRoles.Contains("None"))
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Only users with 'None' role can create a household.", HttpStatusCode.Forbidden);
        }
        
        return ServiceResponseFactory.Success<OperationResultDto>(null, "User is valid for household creation.", HttpStatusCode.OK);
    }
    
    private async Task<ServiceResponse<string>> UploadProfilePicture(CreateHouseholdRequest request)
    {
        // If no file is provided, it's not an error.
        if (request.ProfilePicture == null)
        {
            return ServiceResponseFactory.Success<string>(null, "No profile picture provided.", HttpStatusCode.OK);
        }
    
        try
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "households");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
        
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + request.ProfilePicture.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await request.ProfilePicture.CopyToAsync(fileStream);
            }
        
            string profilePicturePath = Path.Combine("uploads", "households", uniqueFileName).Replace("\\", "/");
        
            // Return success with the file path
            return ServiceResponseFactory.Success(profilePicturePath, "Profile picture uploaded successfully.", HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            // Return a detailed error response on failure
            return ServiceResponseFactory.Error<string>(null, $"Failed to upload profile picture: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    private async Task<ServiceResponse<OperationResultDto>> MakeHouseholdAdminRole(User user)
    {
        var removeRoleResult = await _userManager.RemoveFromRoleAsync(user, "None");
        if (!removeRoleResult.Succeeded)
        {
            var errors = removeRoleResult.Errors.Select(e => e.Description).ToList();
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Failed to remove 'None' role from user.", HttpStatusCode.InternalServerError);
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, "HouseholdAdmin");
        if (!addRoleResult.Succeeded)
        {
            var errors = addRoleResult.Errors.Select(e => e.Description).ToList();
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Failed to assign 'HouseholdAdmin' role to user.", HttpStatusCode.InternalServerError);
        }
        
        var updateUserResult = await _userManager.UpdateAsync(user);
        if (!updateUserResult.Succeeded)
        {
            var errors = updateUserResult.Errors.Select(e => e.Description).ToList();
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Failed to update user information after household creation.", HttpStatusCode.InternalServerError);
        }

        return ServiceResponseFactory.Success<OperationResultDto>(null, "User is valid for household creation.", HttpStatusCode.OK);
    }
}
