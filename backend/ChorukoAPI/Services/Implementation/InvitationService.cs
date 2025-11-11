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

namespace ChorukoAPI.Services.Implementation;

public class InvitationService(DataContext context, IPushNotificationService pushNotificationService, IMapper<Invitation, InvitationResponse> mapper) : IInvitationService
{
    public async Task<ServiceResponse<OperationResultDto>> CreateHouseholdInvitation(string userId, string householdId, string userIdToAdd)
    {
        try
        {
            var userToAdd = await context.Users.FirstOrDefaultAsync(u => u.Id == userIdToAdd);

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

            if (userIdToAdd != userToAdd.Id)
            {
                await pushNotificationService.SendNotificationAsync(userIdToAdd, payload);
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
            return ServiceResponseFactory.Error<InvitationResponse>(null, $"Failed while retrieving invitation: {ex.Message}.", HttpStatusCode.InternalServerError);
        }
    }
}
