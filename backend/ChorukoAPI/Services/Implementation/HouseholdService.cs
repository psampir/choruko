using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Household;
using ChorukoAPI.Contracts.Responses.Household;
using ChorukoAPI.Data;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Identity;
using System.Net; 
using ChorukoAPI.Extensions;
using ChorukoAPI.Contracts.Responses;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using Microsoft.Extensions.Options;
using ChorukoAPI.Mappers;

namespace ChorukoAPI.Services.Implementation;

public class HouseholdService(DataContext _context,
    UserManager<User> _userManager,
    IWebHostEnvironment _webHostEnvironment,
    SignInManager<User> _signInManager,
    IOptions<ImageSettings> _options,
    IHttpContextAccessor _contextAccessor,
    IMapper<Household, HouseholdInfoResponse> _mapper) : IHouseholdService
{
    public async Task<ServiceResponse<HouseholdInfoResponse>> GetHouseholdInfoAsync()
    {
        var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext?.User);

        if (user == null)
        {
            return ServiceResponseFactory.Error(new HouseholdInfoResponse("", "", "", "", "", 0, null, new List<HouseholdMemberResponse>()), "User not found.", HttpStatusCode.NotFound);
        }

        if (string.IsNullOrEmpty(user.HouseholdId))
        {
            return ServiceResponseFactory.Error(new HouseholdInfoResponse("", "", "", "", "", 0, null, new List<HouseholdMemberResponse>()), "User is not part of any household.", HttpStatusCode.BadRequest);
        }

        var household = await _context.Households
            .Include(h => h.Admin)
            .Include(h => h.Participants)
            .FirstOrDefaultAsync(h => h.Id == user.HouseholdId);

        if (household == null)
        {
            return ServiceResponseFactory.Error(new HouseholdInfoResponse("", "", "", "", "", 0, null, new List<HouseholdMemberResponse>()), "Household not found.", HttpStatusCode.NotFound);
        }

        return ServiceResponseFactory.Success(await _mapper.Map(household));
    }


    public async Task<ServiceResponse<OperationResultDto>> CreateHouseholdAsync(CreateHouseholdRequest request, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User not found.", HttpStatusCode.NotFound);
        }
        
        var validationResponse = await ValidateUser(user);
        if (!validationResponse.IsSuccessful)
        {
            return validationResponse;
        }
        
        var uploadResponse = await UploadProfilePicture(request.ProfilePicture);
        if (!uploadResponse.IsSuccessful)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, uploadResponse.Message, uploadResponse.StatusCode);
        }

        string? profilePicturePath = uploadResponse.Result;


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
        
        _context.Households.Add(household);
        var roleResponse = await MakeHouseholdAdminRole(user);
        if (!roleResponse.IsSuccessful)
        {
            return roleResponse;
        }
        
        await _context.SaveChangesAsync(); 
        
        await _signInManager.RefreshSignInAsync(user);
        
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
            "Household was succesfully created.",
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
    
    private async Task<ServiceResponse<string>> UploadProfilePicture(IFormFile? file)
    {
        if (file == null)
        {
            return ServiceResponseFactory.Success<string>(null, "No profile picture provided.", HttpStatusCode.OK);
        }

        var processingResponse = await ValidateAndProcessImage(file);
        
        if (!processingResponse.IsSuccessful)
        {
            return processingResponse;
        }
        
        return ServiceResponseFactory.Success(
            processingResponse.Result!, 
            "Profile picture uploaded and processed successfully.", 
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
        if (! _options.Value.AllowedFormats.Contains(fileExtension))
        {
            return ServiceResponseFactory.Error<string>(null, $"Invalid file format. Only {string.Join(", ", _options.Value.AllowedFormats)} are allowed.", HttpStatusCode.UnsupportedMediaType);
        }

        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "households");
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

            string profilePicturePath = Path.Combine("uploads", "households", uniqueFileName).Replace("\\", "/");
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
    
    public async Task<ServiceResponse<OperationResultDto>> DeleteUserFromHouseholdAsync(string userIdToDelete, string currentUserId)
    {
        var userToDelete = await _userManager.FindByIdAsync(userIdToDelete);
        if (userToDelete == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User to delete not found.", HttpStatusCode.NotFound);
        }

        var currentUser = await _userManager.FindByIdAsync(currentUserId);
        if (currentUser == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Current user not found.", HttpStatusCode.NotFound);
        }
        
        var household = await _context.Households
            .Include(h => h.Participants)
            .FirstOrDefaultAsync(h => h.Id == userToDelete.HouseholdId);

        if (household == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Household not found.", HttpStatusCode.NotFound);
        }
        
        if (userToDelete.HouseholdId != household.Id)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User is not a participant of this household.", HttpStatusCode.BadRequest);
        }
        
        var userRoles = await _userManager.GetRolesAsync(userToDelete);
        if (userRoles.Contains("HouseholdAdmin"))
        {
            int adminCount = 0;
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
                return ServiceResponseFactory.Error<OperationResultDto>(null, "Cannot leave. A household must have at least one admin.", HttpStatusCode.Forbidden);
            }
        }
        
        userToDelete.HouseholdId = null;
        userToDelete.Household = null;
        
        await _userManager.RemoveFromRolesAsync(userToDelete, await _userManager.GetRolesAsync(userToDelete));
        await _userManager.AddToRoleAsync(userToDelete, "None");
        
        _context.Users.Update(userToDelete);
        await _context.SaveChangesAsync();

        var response = new OperationResultDto
        {
            Operation = "DeleteUserFromHousehold",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "HouseholdId", household.Id },
                { "UserId", userToDelete.Id },
                { "UserName", userToDelete.FirstName + " " + userToDelete.LastName }
            }
        };

        return ServiceResponseFactory.Success(
            response,
            "User was successfully removed from the household.",
            HttpStatusCode.OK
        );
    }
    
    public async Task<ServiceResponse<OperationResultDto>> AddUserToHouseholdAsync(string userIdToAdd, string currentUserId)
    {
        var currentUser = await _userManager.FindByIdAsync(currentUserId);
        if (currentUser == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Current user not found.", HttpStatusCode.NotFound);
        }

        var userToAdd = await _userManager.FindByIdAsync(userIdToAdd);
        if (userToAdd == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User to add not found.", HttpStatusCode.NotFound);
        }
        
        var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
        if (currentUser.HouseholdId == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Current user does not belong to a household.", HttpStatusCode.BadRequest);
        }
        
        var household = await _context.Households
            .Include(h => h.Participants)
            .FirstOrDefaultAsync(h => h.Id == currentUser.HouseholdId);
            
        if (household == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Household not found.", HttpStatusCode.NotFound);
        }
        
        if (userToAdd.HouseholdId != null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User to add already belongs to a household.", HttpStatusCode.Conflict);
        }
        
        var userToAddRoles = await _userManager.GetRolesAsync(userToAdd);
        if (!userToAddRoles.Contains("None"))
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Only users with the 'None' role can be added to a household.", HttpStatusCode.Forbidden);
        }
        
        userToAdd.HouseholdId = household.Id;
        household.Participants.Add(userToAdd);
        
        var removeRoleResult = await _userManager.RemoveFromRoleAsync(userToAdd, "None");
        if (!removeRoleResult.Succeeded)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Failed to remove 'None' role from user.", HttpStatusCode.InternalServerError);
        }

        var addRoleResult = await _userManager.AddToRoleAsync(userToAdd, "Participant");
        if (!addRoleResult.Succeeded)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Failed to assign 'Participant' role to user.", HttpStatusCode.InternalServerError);
        }

        _context.Users.Update(userToAdd);
        
        await _context.SaveChangesAsync();
        
        var response = new OperationResultDto
        {
            Operation = "AddUserToHousehold",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "HouseholdId", household.Id },
                { "UserId", userToAdd.Id },
                { "UserName", userToAdd.FirstName + " " + userToAdd.LastName }
            }
        };

        return ServiceResponseFactory.Success(
            response,
            "User was successfully added to the household.",
            HttpStatusCode.OK
        );
    }
    
    public async Task<ServiceResponse<OperationResultDto>> DeleteHouseholdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        
        var userRoles = await _userManager.GetRolesAsync(user);
        if (!userRoles.Contains("HouseholdAdmin"))
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Only household admins can delete a household.", HttpStatusCode.Forbidden);
        }
        
        var householdId = user.HouseholdId;

        var household = await _context.Households
            .Include(h => h.Participants)
            .FirstOrDefaultAsync(h => h.Id == householdId);
        
        if (household == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Household not found.", HttpStatusCode.NotFound);
        }

        if (!string.IsNullOrEmpty(household.ProfilePicturePath))
        {
            DeleteProfilePicture(household.ProfilePicturePath);
        }

        var participantsToUpdate = household.Participants.ToList();
        foreach (var participant in participantsToUpdate)
        {
            participant.HouseholdId = null;
            await _userManager.RemoveFromRolesAsync(participant, await _userManager.GetRolesAsync(participant));
            await _userManager.AddToRoleAsync(participant, "None");
        }

        _context.Households.Remove(household);
        
        await _signInManager.RefreshSignInAsync(user);

        await _context.SaveChangesAsync();
        var response = new OperationResultDto
        {
            Operation = "DeleteHousehold",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "HouseholdId", household.Id },
                { "HouseholdName", household.Name }
            }
        };

        return ServiceResponseFactory.Success(
            response,
            "Household was successfully deleted.",
            HttpStatusCode.OK
        );
    }
     
    private void DeleteProfilePicture(string filePath)
    {
        string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath);
        System.IO.File.Delete(fullPath);
    }
    
    public async Task<ServiceResponse<OperationResultDto>> RenameHouseholdAsync(string userId, string newName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User not found.", HttpStatusCode.NotFound);
        }
    
        var userRoles = await _userManager.GetRolesAsync(user);
        if (!userRoles.Contains("HouseholdAdmin"))
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Only household admins can rename a household.", HttpStatusCode.Forbidden);
        }
        
        var householdId = user.HouseholdId;
        if (string.IsNullOrEmpty(householdId))
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User does not belong to a household.", HttpStatusCode.BadRequest);
        }

        var household = await _context.Households
            .FirstOrDefaultAsync(h => h.Id == householdId);

        if (household == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Household not found.", HttpStatusCode.NotFound);
        }
        
        var oldName = household.Name;
        household.Name = newName;
        
        _context.Households.Update(household);
        await _context.SaveChangesAsync();
        
        var response = new OperationResultDto
        {
            Operation = "RenameHousehold",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "HouseholdId", household.Id },
                { "OldHouseholdName", oldName },
                { "NewHouseholdName", household.Name }
            }
        };

        return ServiceResponseFactory.Success(
            response,
            "Household was successfully renamed.",
            HttpStatusCode.OK
        );
    }   
    public async Task<ServiceResponse<OperationResultDto>> LeaveHouseholdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User not found.", HttpStatusCode.NotFound);
        }
        
        if (string.IsNullOrEmpty(user.HouseholdId))
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User does not belong to a household.", HttpStatusCode.BadRequest);
        }
        
        var userRoles = await _userManager.GetRolesAsync(user);
        var household = await _context.Households.Include(h => h.Participants).FirstOrDefaultAsync(h => h.Id == user.HouseholdId);

        if (household == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Household not found.", HttpStatusCode.NotFound);
        }
        
        if (userRoles.Contains("HouseholdAdmin"))
        {
            int adminCount = 0;
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
                return ServiceResponseFactory.Error<OperationResultDto>(null, "Cannot leave. A household must have at least one admin.", HttpStatusCode.Forbidden);
            }
        }
        
        user.HouseholdId = null;
        await _userManager.RemoveFromRolesAsync(user, userRoles);
        await _userManager.AddToRoleAsync(user, "None");
        
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        await _signInManager.RefreshSignInAsync(user);
        
        var response = new OperationResultDto
        {
            Operation = "LeaveHousehold",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "HouseholdId", household.Id },
                { "UserId", user.Id }
            }
        };

        return ServiceResponseFactory.Success(
            response,
            "User successfully left the household.",
            HttpStatusCode.OK
        );
    }
    
    public async Task<ServiceResponse<OperationResultDto>> ChangeHouseholdPictureAsync(ChangeHouseholdPictureRequest request, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User not found.", HttpStatusCode.NotFound);
        }
        
        var household = await _context.Households
            .FirstOrDefaultAsync(h => h.Id == user.HouseholdId);
        
        if (household == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Household not found.", HttpStatusCode.NotFound);
        }
        
        var oldProfilePicturePath = household.ProfilePicturePath;
        
        var uploadResponse = await UploadProfilePicture(request.ProfilePicture);

        if (!uploadResponse.IsSuccessful)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, uploadResponse.Message, uploadResponse.StatusCode);
        }
        
        string newProfilePicturePath = uploadResponse.Result!;
        
        household.ProfilePicturePath = newProfilePicturePath;

        try
        {
            await _context.SaveChangesAsync();
            
            if (!string.IsNullOrEmpty(oldProfilePicturePath))
            {
                DeleteProfilePicture(oldProfilePicturePath); 
            }

            var response = new OperationResultDto
            {
                Operation = "ChangeHouseholdPicture",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "HouseholdId", household.Id },
                    { "NewProfilePicturePath", newProfilePicturePath }
                }
            };

            return ServiceResponseFactory.Success(
                response,
                "Household picture was successfully changed.",
                HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            DeleteProfilePicture(newProfilePicturePath); 
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"Failed to save changes to the database: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
    public async Task<ServiceResponse<OperationResultDto>> SetHouseholdAdminAsync(string userId, string newAdminId)
    {
        var currentAdmin = await _userManager.FindByIdAsync(userId);
        var newAdmin = await _userManager.FindByIdAsync(newAdminId);

        if (currentAdmin == null || newAdmin == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "One or both users not found.", HttpStatusCode.NotFound);
        }

        if (currentAdmin.HouseholdId != newAdmin.HouseholdId || string.IsNullOrEmpty(currentAdmin.HouseholdId))
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Both users must belong to the same household.", HttpStatusCode.BadRequest);
        }

        var currentAdminRoles = await _userManager.GetRolesAsync(currentAdmin);
        if (!currentAdminRoles.Contains("HouseholdAdmin"))
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Current user is not a household admin.", HttpStatusCode.Forbidden);
        }

        var newAdminRoles = await _userManager.GetRolesAsync(newAdmin);
        
        if (newAdminRoles.Contains("HouseholdAdmin"))
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "User is already an admin.", HttpStatusCode.Conflict);
        }
        if (!newAdminRoles.Contains("Participant"))
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "New admin must have the 'Participant' role.", HttpStatusCode.Forbidden);
        }
        
        var removeOldAdminRoleResult = await _userManager.RemoveFromRoleAsync(currentAdmin, "HouseholdAdmin");
        if (!removeOldAdminRoleResult.Succeeded)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Failed to demote current admin: " + string.Join(", ", removeOldAdminRoleResult.Errors.Select(e => e.Description)), HttpStatusCode.InternalServerError);
        }
        
        var addOldAdminRoleResult = await _userManager.AddToRoleAsync(currentAdmin, "Participant");
        if (!addOldAdminRoleResult.Succeeded)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Failed to assign 'Participant' role to old admin: " + string.Join(", ", addOldAdminRoleResult.Errors.Select(e => e.Description)), HttpStatusCode.InternalServerError);
        }
        
        var promoteNewAdminResult = await _userManager.AddToRoleAsync(newAdmin, "HouseholdAdmin");
        if (!promoteNewAdminResult.Succeeded)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "Failed to promote new admin: " + string.Join(", ", promoteNewAdminResult.Errors.Select(e => e.Description)), HttpStatusCode.InternalServerError);
        }
        
        await _userManager.RemoveFromRoleAsync(newAdmin, "Participant"); 
        
        var household = await _context.Households.FirstOrDefaultAsync(h => h.Id == currentAdmin.HouseholdId);
        if (household != null)
        {
            household.AdminId = newAdminId;
            _context.Households.Update(household);
        }
        
        await _context.SaveChangesAsync();
        await _signInManager.RefreshSignInAsync(currentAdmin);
        await _signInManager.RefreshSignInAsync(newAdmin);

        var response = new OperationResultDto
        {
            Operation = "SetHouseholdAdmin",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "OldAdminId", currentAdmin.Id },
                { "NewAdminId", newAdmin.Id },
                { "HouseholdId", household?.Id } 
            }
        };

        return ServiceResponseFactory.Success(
            response,
            "Household admin was successfully set and roles were swapped.",
            HttpStatusCode.OK
        );
    }
}
