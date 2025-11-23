using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts;
using ChorukoAPI.Extensions;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ChorukoAPI.Data;
using ChorukoAPI.Contracts.Responses.Invitation;
using ChorukoAPI.Mappers;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.AspNetCore.Identity;

namespace ChorukoAPI.Services.Implementation;

public class InvitationService(DataContext context,
    IPushNotificationService pushNotificationService,
    IMapper<Invitation, InvitationResponse> mapper,
    UserManager<User> userManager) : IInvitationService
{
    public async Task<ServiceResponse<OperationResultDto>> CreateHouseholdInvitation(string userId, string householdId, string userEmail)
    {
        try
        {
            var userToAdd = await context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (userToAdd == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "No users were found", HttpStatusCode.NotFound);
            }

            var household = await context.Households.FirstOrDefaultAsync(h => h.Id == householdId);

            if (household == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "No households were found", HttpStatusCode.NotFound);
            }

            var invitation = new Invitation
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = userId,
                Receiver = userToAdd,
                Household = household,
                Status = InvitationStatus.Pending,
                SentAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            };

            context.Invitations.Add(invitation);
            await context.SaveChangesAsync();

            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                title = $"👋 You were invited to household by {household.Admin.FirstName}!",
                body = $"Household name: {household.Name}!",
                icon = "/icons/icon-192x192.png",
                badge = "/icons/icon-96x96.png",
                url = "/"
            });

            if (userId != userToAdd.Id)
            {
                await pushNotificationService.SendNotificationAsync(userToAdd.Id, payload);
            }


            var response = new OperationResultDto
            {
                Operation = "CreateHouseholdInvitation",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "HouseholdId", household.Id },
                    { "UserId", userId },
                    { "UserName", household.Admin.FirstName + " " + household.Admin.LastName }
                }
            };

            return ServiceResponseFactory.Success(
                response,
                "User was successfully removed from the household.",
                HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"Failed while creating invitation: {ex.Message}.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ServiceResponse<List<InvitationResponse>>> GetAllInvitations(string userId)
    {
        try
        {
            var invitations = await context.Invitations
                .Include(i => i.Receiver)
                .Include(i => i.Sender)
                .Include(i => i.Household)
                .Where(i => i.ReceiverId == userId)
                .ToListAsync();

            var result = new List<InvitationResponse>();

            foreach (var invitation in invitations)
            {
                result.Add(await mapper.Map(invitation));
            }

            return ServiceResponseFactory.Success(result, "Successful retrieval of the invitation");
        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<List<InvitationResponse>>(null, $"Failed while retrieving invitation: {ex.Message}.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ServiceResponse<InvitationResponse>> GetInvitation(string invitationId)
    {
        try
        {
            var invitation = await context.Invitations
                .Include(i => i.Receiver)
                .Include(i => i.Sender)
                .Include(i => i.Household)
                .FirstOrDefaultAsync(i => i.Id == invitationId);

            if (invitation == null)
            {
                return ServiceResponseFactory.Error<InvitationResponse>(null, "No invitation were found", HttpStatusCode.NotFound);
            }

            var result = await mapper.Map(invitation);

            return ServiceResponseFactory.Success(result, "Successful retrieval of the invitation");
        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<InvitationResponse>(null, $"Failed while retrieving invitation info: {ex.Message}.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ServiceResponse<OperationResultDto>> AcceptInvitation(string invitationId)
    {
        try
        {
            var invitation = await context.Invitations
                .Include(i => i.Receiver)
                .Include(i => i.Sender)
                .Include(i => i.Household)
                .FirstOrDefaultAsync(i => i.Id == invitationId);

            if (invitation == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "No invitation were found", HttpStatusCode.NotFound);
            }

            var household = await context.Households.FirstOrDefaultAsync(h => h.Id == invitation.HouseholdId);

            if (household == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "No household were found. Maybe it was deleted, or does not exist", HttpStatusCode.NotFound);
            }

            var newParticipant = await context.Users.FirstOrDefaultAsync(u => u.Id == invitation.ReceiverId);

            if (newParticipant == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "Error retrieving your data", HttpStatusCode.NotFound);
            }

            if (newParticipant.HouseholdId != null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "You already belong to a household.", HttpStatusCode.Conflict);
            }

            var userToAddRoles = await userManager.GetRolesAsync(newParticipant);
            if (!userToAddRoles.Contains("None"))
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "Only users with the 'None' role can be added to a household.", HttpStatusCode.Forbidden);
            }

            var removeRoleResult = await userManager.RemoveFromRoleAsync(newParticipant, "None");
            if (!removeRoleResult.Succeeded)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "Failed to remove 'None' role from user.", HttpStatusCode.InternalServerError);
            }

            var addRoleResult = await userManager.AddToRoleAsync(newParticipant, "Participant");
            if (!addRoleResult.Succeeded)
            {
                await userManager.AddToRoleAsync(newParticipant, "None");
                return ServiceResponseFactory.Error<OperationResultDto>(null, "Failed to assign 'Participant' role to user.", HttpStatusCode.InternalServerError);
            }

            invitation.Status = InvitationStatus.Accepted;
            newParticipant.Household = household;
            household.Participants.Add(newParticipant);

            context.Users.Update(newParticipant);

            await context.SaveChangesAsync();

            var response = new OperationResultDto
            {
                Operation = "AcceptInvitation",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "HouseholdId", household.Id },
                    { "UserId", newParticipant.Id },
                    { "UserName", newParticipant.FirstName + " " + newParticipant.LastName }
                }
            };

            return ServiceResponseFactory.Success(
                response,
                "User was successfully added to the household.",
                HttpStatusCode.OK
            );

        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"Failed while accepting invitation: {ex.Message}.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ServiceResponse<OperationResultDto>> DeclineInvitation(string invitationId)
    {
        try
        {
            var invitation = await context.Invitations
                .Include(i => i.Receiver)
                .Include(i => i.Sender)
                .Include(i => i.Household)
                .FirstOrDefaultAsync(i => i.Id == invitationId);

            if (invitation == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "No invitation were found", HttpStatusCode.NotFound);
            }

            if (invitation.Status == InvitationStatus.Accepted)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "You accepted this invitation", HttpStatusCode.BadRequest);
            }

            invitation.Status = InvitationStatus.Declined;

            await context.SaveChangesAsync();

            var response = new OperationResultDto
            {
                Operation = "DeclineInvitation",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "InvitationId", invitation.Id },
                    { "UserId", invitation.ReceiverId },
                    { "UserName", invitation.Receiver.FirstName + " " + invitation.Receiver.LastName }
                }
            };

            return ServiceResponseFactory.Success(
                response,
                "User successfully declined the invitation.",
                HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"Failed while declining invitation: {ex.Message}.", HttpStatusCode.InternalServerError);
        }
    }

}
