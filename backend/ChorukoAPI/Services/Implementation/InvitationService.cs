using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts;
using ChorukoAPI.Extensions;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ChorukoAPI.Data;

namespace ChorukoAPI.Services.Implementation;

public class InvitationService(DataContext context, IPushNotificationService pushNotificationService) : IInvitationService
{
    public async Task<ServiceResponse<OperationResultDto>> CreateHouseholdInvitation(string userId, string householdId, string userIdToAdd)
    {
        try
        {
            var userToAdd = await context.Users.FirstOrDefaultAsync(u => u.Id == userIdToAdd);

            if (userToAdd == null)
            {
                ServiceResponseFactory.Error<OperationResultDto>(null, "No users were found", HttpStatusCode.NotFound);
            }

            var household = await context.Households.FirstOrDefaultAsync(h => h.Id == householdId);

            var invitation = new Invitation
            {
                SenderId = userId,
                Receiver = userToAdd,
                Household = household,
                Status = InvitationStatus.Pending,
                SentAt = DateTime.UtcNow,
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
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"Failed while creating task: {ex.Message}.", HttpStatusCode.InternalServerError);
        }
    }

}
