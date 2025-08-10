using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Household;
using ChorukoAPI.Contracts.Responses.Household;
using ChorukoAPI.Data;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net; 
using Microsoft.AspNetCore.Hosting; 
using System.IO; 
using ChorukoAPI.Extensions;
using ChorukoAPI.Contracts.Responses;


namespace ChorukoAPI.Services.Implementation;

public class HouseholdService(DataContext _context, UserManager<User> _userManager, IWebHostEnvironment _webHostEnvironment) : IHouseholdService
{
    public async Task<ServiceResponse<HouseholdResponse>> CreateHouseholdAsync(CreateHouseholdRequest request, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResponseFactory.Error<HouseholdResponse>(null, "User not found.", HttpStatusCode.NotFound);
        }

        // Ensure the user is not already part of a household
        if (user.HouseholdId != null)
        {
            return ServiceResponseFactory.Error<HouseholdResponse>(null, "User already belongs to a household.", HttpStatusCode.Conflict);
        }

        // Verify that the user has the "None" role before allowing household creation
        var userRoles = await _userManager.GetRolesAsync(user);
        if (!userRoles.Contains("None"))
        {
            return ServiceResponseFactory.Error<HouseholdResponse>(null, "Only users with 'None' role can create a household.", HttpStatusCode.Forbidden);
        }

        string? profilePicturePath = null;
        if (request.ProfilePicture != null)
        {
            try
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "households");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder); // Create directory if it doesn't exist
                }
                
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + request.ProfilePicture.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ProfilePicture.CopyToAsync(fileStream);
                }
                
                profilePicturePath = Path.Combine("uploads", "households", uniqueFileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                return ServiceResponseFactory.Error<HouseholdResponse>(null, $"Failed to upload profile picture: {ex.Message}", HttpStatusCode.InternalServerError);
            }
        }

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
        
        var removeRoleResult = await _userManager.RemoveFromRoleAsync(user, "None");
        if (!removeRoleResult.Succeeded)
        {
            var errors = removeRoleResult.Errors.Select(e => e.Description).ToList();
            return ServiceResponseFactory.Error<HouseholdResponse>(null, "Failed to remove 'None' role from user.", HttpStatusCode.InternalServerError);
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, "HouseholdAdmin");
        if (!addRoleResult.Succeeded)
        {
            var errors = addRoleResult.Errors.Select(e => e.Description).ToList();
            return ServiceResponseFactory.Error<HouseholdResponse>(null, "Failed to assign 'HouseholdAdmin' role to user.", HttpStatusCode.InternalServerError);
        }
        
        var updateUserResult = await _userManager.UpdateAsync(user);
        if (!updateUserResult.Succeeded)
        {
             var errors = updateUserResult.Errors.Select(e => e.Description).ToList();
             return ServiceResponseFactory.Error<HouseholdResponse>(null, "Failed to update user information after household creation.", HttpStatusCode.InternalServerError);
        }

        await _context.SaveChangesAsync(); 
        
        var adminUser = await _userManager.FindByIdAsync(household.AdminId);

        // Construct the full public URL for the profile picture to send to the frontend
        string? fullProfilePictureUrl = null;
        if (!string.IsNullOrEmpty(household.ProfilePicturePath))
        {
            var baseUrl = "http://localhost:5180/"; // Adjust this to your actual API base URL
            fullProfilePictureUrl = $"{baseUrl}{household.ProfilePicturePath}";
        }

        // Prepare the HouseholdResponse DTO for the frontend
        var response = new HouseholdResponse
        {
            Id = household.Id,
            Name = household.Name,
            AdminId = household.AdminId,
            AdminFirstName = adminUser?.FirstName, // Use optional chaining for safety
            AdminLastName = adminUser?.LastName,
            ParticipantsCount = household.Participants.Count, // Should be 1 at creation
            ProfilePictureUrl = fullProfilePictureUrl,
        };

        return ServiceResponseFactory.Success(response, "Household created successfully and user role updated.", HttpStatusCode.Created);
    }
}
