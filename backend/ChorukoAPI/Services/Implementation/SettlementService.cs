using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Settlement;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Settlement;
using ChorukoAPI.Data;
using ChorukoAPI.Extensions;
using ChorukoAPI.Mappers;
using ChorukoAPI.Models;
using ChorukoAPI.Services.Abstraction;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Net;

namespace ChorukoAPI.Services.Implementation;

public class SettlementService(
    DataContext _context,
    IMapper<Settlement, SettlementResponse> _mapper,
    IMapper<Settlement, SettlementWithoutUsersResponse> mapper,
    IPushNotificationService pushNotificationService) : ISettlementService
{
    public async Task<ServiceResponse<OperationResultDto>> CreateSettlement(string householdId, CreateSettlementRequest request, string userId)
    {
        try
        {
            var household = await _context.Households
                .Include(h => h.Participants)
                .FirstOrDefaultAsync(h => h.Id == householdId);

            if (household == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(
                    null,
                    "Household not found",
                    HttpStatusCode.NotFound
                );
            }

            if (request.Amount <= 0)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(
                    null,
                    "Amount must be greater than 0",
                    HttpStatusCode.BadRequest
                );
            }

            if (request.UnsettledUsers.Contains(userId))
                return ServiceResponseFactory.Error<OperationResultDto>(null, "Creator cannot assign himself to settlement", HttpStatusCode.BadRequest);

            var allParticipants = household.Participants ?? new List<User>();

            var requestedUnsettledIds = request.UnsettledUsers
                .Where(id => id != userId)
                .ToHashSet();

            var participantIds = allParticipants.Select(p => p.Id).ToHashSet();
            var invalidIds = requestedUnsettledIds.Except(participantIds).ToList();
            if (invalidIds.Count > 0)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(
                    null,
                    $"Some users are not participants of the household: {string.Join(", ", invalidIds)}",
                    HttpStatusCode.BadRequest
                );
            }

            var unsettledUsers = allParticipants
                .Where(u => requestedUnsettledIds.Contains(u.Id))
                .ToList();

            var creator = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            var settlement = new Settlement
            {
                Id = Guid.NewGuid().ToString(),
                HouseholdId = householdId,
                Creator = creator,
                DateOfCreation = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                Description = request.Description,
                Amount = request.Amount,
                Status = SettlementStatus.Pending,
                UnsettledUsers = unsettledUsers,
                SettledUsers = new List<User>()
            };

            _context.Settlements.Add(settlement);
            await _context.SaveChangesAsync();

            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                title = $"👋 {creator.FirstName} requested money from you!",
                body = $"{request.Description}, amount: {request.Amount}!",
                icon = "/icons/icon-192x192.png",
                badge = "/icons/icon-96x96.png",
                url = "/"
            });

            await NotificationHandler.SendNotificationAsync(unsettledUsers.Select(receiver => receiver.Id).ToList(), payload);

            var result = new OperationResultDto
            {
                Operation = "CreateSettlement",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["SettlementId"] = settlement.Id,
                    ["UnsettledCount"] = unsettledUsers.Count,
                    ["SettledCount"] = settlement.SettledUsers.Count
                }
            };

            return ServiceResponseFactory.Success(
                result,
                "Settlement created successfully",
                HttpStatusCode.Created
            );
        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(
                null,
                $"Failed to create settlement: {ex.Message}",
                HttpStatusCode.InternalServerError
            );
        }
    }

    public async Task<ServiceResponse<OperationResultDto>> EditSettlement(string settlementId, EditSettlementRequest request, string userId)
    {
        try
        {
            var settlement = await _context.Settlements
                .Include(s => s.UnsettledUsers)
                .Include(s => s.SettledUsers)
                .Include(s => s.Household).ThenInclude(h => h.Participants)
                .FirstOrDefaultAsync(s => s.Id == settlementId);

            if (settlement is null)
                return ServiceResponseFactory.Error<OperationResultDto>(null, "Settlement not found", HttpStatusCode.NotFound);

            if (settlement.CreatorId != userId)
                return ServiceResponseFactory.Error<OperationResultDto>(null, "User is not a creator of the settlement", HttpStatusCode.NotFound);


            if (request.UnsettledUsers.Contains(userId))
                return ServiceResponseFactory.Error<OperationResultDto>(null, "Creator cannot assign himself to settlement", HttpStatusCode.BadRequest);

            if (request.Amount <= 0)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(
                    null,
                    "Amount must be greater than 0",
                    HttpStatusCode.BadRequest
                );
            }

            var participants = settlement.Household?.Participants ?? new List<User>();
            var participantIds = participants.Select(p => p.Id).ToHashSet(StringComparer.Ordinal);
            var creatorId = settlement.CreatorId;

            var reqUnsettled = request.UnsettledUsers?.ToHashSet(StringComparer.Ordinal);

            HashSet<string> targetUnsettled;

            if (reqUnsettled is not null)
            {
                targetUnsettled = reqUnsettled;

                var invalid = targetUnsettled.Where(id => !participantIds.Contains(id)).Distinct().ToList();
                if (invalid.Count > 0)
                    return ServiceResponseFactory.Error<OperationResultDto>(null,
                        $"Some users are not participants of the household: {string.Join(", ", invalid)}",
                        HttpStatusCode.BadRequest);

                var currentSettledIds = settlement.SettledUsers
                    .Select(u => u.Id)
                    .ToHashSet(StringComparer.Ordinal);
            }
            else
            {
                targetUnsettled = settlement.UnsettledUsers?.Select(u => u.Id).ToHashSet(StringComparer.Ordinal)
                                    ?? new HashSet<string>(StringComparer.Ordinal);
            }

            var newUnsettled = participants.Where(p => targetUnsettled.Contains(p.Id)).ToList();

            settlement.UnsettledUsers = newUnsettled;
            settlement.Description = request.Description;
            settlement.Amount = request.Amount;
            settlement.SettledUsers = new List<User>();

            await _context.SaveChangesAsync();


            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                title = "👋 Settlement was updated!",
                body = $"{request.Description}, amount: {request.Amount}!",
                icon = "/icons/icon-192x192.png",
                badge = "/icons/icon-96x96.png",
                url = "/"
            });

            await NotificationHandler.SendNotificationAsync(settlement.UnsettledUsers.Select(receiver => receiver.Id).ToList(), payload);

            var op = new OperationResultDto
            {
                Operation = "EditSettlement",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["SettlementId"] = settlement.Id,
                    ["UnsettledCount"] = newUnsettled.Count,
                    ["SettledCount"] = settlement.SettledUsers.Count
                }
            };

            return ServiceResponseFactory.Success(op, "Settlement updated successfully", HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, $"Failed to edit settlement: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<OperationResultDto>> DeleteSettlement(string settlementId)
    {
        var settlement = await _context.Settlements.FirstOrDefaultAsync(s => s.Id == settlementId);
        if (settlement == null)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(null, "No settlement found", HttpStatusCode.NotFound);
        }

        _context.Settlements.Remove(settlement);
        await _context.SaveChangesAsync();

        var response = new OperationResultDto
        {
            Operation = "DeleteSettlement",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "Settlement description", settlement.Description}
            }
        };

        return ServiceResponseFactory.Success(response, "Settlement was successfully deleted");
    }

    public async Task<ServiceResponse<List<SettlementWithoutUsersResponse>>> GetAllSettlements(string householdId)
    {
        try
        {
            var settlements = await _context.Settlements
                .Include(s => s.Creator)
                .Where(s => s.HouseholdId == householdId)
                .AsNoTracking()
                .ToListAsync();

            if (settlements.Count == 0)
            {
                return ServiceResponseFactory.Success(new List<SettlementWithoutUsersResponse>(), "No settlements");
            }

            var result = new List<SettlementWithoutUsersResponse>();

            foreach (var settlement in settlements)
            {
                result.Add(await mapper.Map(settlement));
            }

            return ServiceResponseFactory.Success(result, "Settlements retrieved successfully");
        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<List<SettlementWithoutUsersResponse>>(
                null,
                $"Failed to retrieve household settlements: {ex.Message}"
                );
        }
    }

    public async Task<ServiceResponse<SettlementResponse>> GetSettlement(string settlementId)
    {
        try
        {
            var settlement = await _context.Settlements.FirstOrDefaultAsync(s => s.Id == settlementId);

            if (settlement == null)
            {
                return ServiceResponseFactory.Error<SettlementResponse>(null, "No settlement found", HttpStatusCode.NotFound);
            }

            var result = await _mapper.Map(settlement);

            return ServiceResponseFactory.Success(result, "Settlement retrieved successfully");
        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<SettlementResponse>(
                null,
                $"Failed to retrieve household settlements: {ex.Message}"
                );
        }
    }

    public async Task<ServiceResponse<OperationResultDto>> MarkAsDone(string settlementId, string userId)
    {
        try
        {
            var settlement = await _context.Settlements
                .Include(s => s.SettledUsers)
                .Include(s => s.UnsettledUsers)
                .FirstOrDefaultAsync(s => s.Id == settlementId);

            if (settlement == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "No settlement found", HttpStatusCode.NotFound);
            }

            if (settlement.CreatorId != userId)
                return ServiceResponseFactory.Error<OperationResultDto>(null, "User is not a creator of the settlement", HttpStatusCode.NotFound);

            settlement.Status = SettlementStatus.Completed;
            await _context.SaveChangesAsync();

            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                title = "👋 Settlement was completed!",
                body = $"{settlement.Description}, amount: {settlement.Amount}!",
                icon = "/icons/icon-192x192.png",
                badge = "/icons/icon-96x96.png",
                url = "/"
            });

            var notificationTasks = new List<Task>();

            if (settlement.SettledUsers != null)
            {
                notificationTasks.AddRange(
                    settlement.SettledUsers.Select(receiver =>
                        pushNotificationService.SendNotificationAsync(receiver.Id, payload))
                );
            }

            if (settlement.UnsettledUsers != null)
            {
                notificationTasks.AddRange(
                    settlement.UnsettledUsers.Select(receiver =>
                        pushNotificationService.SendNotificationAsync(receiver.Id, payload))
                );
            }

            if (notificationTasks.Any())
            {
                await Task.WhenAll(notificationTasks);
            }

            var result = new OperationResultDto
            {
                Operation = "MarkAsDone",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object> { ["SettlementId"] = settlement.Id }
            };

            return ServiceResponseFactory.Success(result, "Settlement marked as completed successful", HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(
                null,
                $"Failed to change settlement status: {ex.Message}",
                HttpStatusCode.InternalServerError
            );
        }
    }

    public async Task<ServiceResponse<OperationResultDto>> UnMarkAsDone(string settlementId, string userId)
    {
        try
        {
            var settlement = await _context.Settlements
                .Include(s => s.SettledUsers)
                .Include(s => s.UnsettledUsers)
                .FirstOrDefaultAsync(s => s.Id == settlementId);

            if (settlement == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "No settlement found", HttpStatusCode.NotFound);
            }

            if (settlement.CreatorId != userId)
                return ServiceResponseFactory.Error<OperationResultDto>(null, "User is not a creator of the settlement", HttpStatusCode.NotFound);

            settlement.Status = SettlementStatus.Pending;
            await _context.SaveChangesAsync();

            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                title = "👋 Settlement was reopened",
                body = $"{settlement.Description}, amount: {settlement.Amount}!",
                icon = "/icons/icon-192x192.png",
                badge = "/icons/icon-96x96.png",
                url = "/"
            });

            var notificationTasks = new List<Task>();

            if (settlement.SettledUsers != null)
            {
                notificationTasks.AddRange(
                    settlement.SettledUsers.Select(receiver =>
                        pushNotificationService.SendNotificationAsync(receiver.Id, payload))
                );
            }

            if (settlement.UnsettledUsers != null)
            {
                notificationTasks.AddRange(
                    settlement.UnsettledUsers.Select(receiver =>
                        pushNotificationService.SendNotificationAsync(receiver.Id, payload))
                );
            }

            if (notificationTasks.Any())
            {
                await Task.WhenAll(notificationTasks);
            }

            var result = new OperationResultDto
            {
                Operation = "UnMarkAsDone",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["SettlementId"] = settlement.Id
                }
            };

            return ServiceResponseFactory.Success(
                result,
                "Settlement unmarked as completed successfully",
                HttpStatusCode.Created
            );
        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(
                null,
                $"Failed to change settlement status: {ex.Message}",
                HttpStatusCode.InternalServerError
            );
        }
    }

    public async Task<ServiceResponse<OperationResultDto>> MarkAsPaid(string settlementId, string userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "User not found.", HttpStatusCode.NotFound);
            }

            var settlement = await _context.Settlements.Include(s => s.SettledUsers).Include(s => s.UnsettledUsers).FirstOrDefaultAsync(s => s.Id == settlementId);

            if (user == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "No Settlement found.", HttpStatusCode.NotFound);
            }

            if (!settlement.UnsettledUsers.Contains(user))
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "User does not participate in settlement.", HttpStatusCode.NotFound);
            }

            if (settlement.CreatorId == userId)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "Creator already paid for settlement.", HttpStatusCode.NotFound);
            }

            settlement.UnsettledUsers.Remove(user);
            settlement.SettledUsers.Add(user);

            await _context.SaveChangesAsync();

            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                title = $"👋 {user.FirstName} completed the payment",
                body = $"{settlement.Description}, amount: {settlement.Amount}!",
                icon = "/icons/icon-192x192.png",
                badge = "/icons/icon-96x96.png",
                url = "/"
            });

            await pushNotificationService.SendNotificationAsync(settlement.CreatorId, payload);

            var result = new OperationResultDto
            {
                Operation = "MarkAsPaid",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
            {
                { "Settlement description", settlement.Description},
                { "User fist name", user.FirstName },
                { "User last name", user.LastName }
            }
            };

            return ServiceResponseFactory.Success(result, "Settlement was successfully paid");
        }
        catch(Exception ex)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(
                null,
                $"Failed to change settlement status: {ex.Message}",
                HttpStatusCode.InternalServerError
            );
        }
    }


    public async Task<ServiceResponse<OperationResultDto>> MarAsUnPaid(string settlementId, string userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "User not found.", HttpStatusCode.NotFound);
            }

            var settlement = await _context.Settlements.Include(s => s.SettledUsers).Include(s => s.UnsettledUsers).FirstOrDefaultAsync(s => s.Id == settlementId);

            if (user == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "No Settlement found.", HttpStatusCode.NotFound);
            }

            if (settlement.UnsettledUsers.Contains(user))
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "User does not participate in settlement.", HttpStatusCode.NotFound);
            }

            if(settlement.CreatorId == userId)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "Creator already paid for settlement.", HttpStatusCode.NotFound);
            }

            settlement.SettledUsers.Remove(user);
            settlement.UnsettledUsers.Add(user);

            await _context.SaveChangesAsync();


            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                title = $"👋 {user.FirstName} cancelled the payment",
                body = $"{settlement.Description}, amount: {settlement.Amount}!",
                icon = "/icons/icon-192x192.png",
                badge = "/icons/icon-96x96.png",
                url = "/"
            });

            await pushNotificationService.SendNotificationAsync(settlement.CreatorId, payload);

            var result = new OperationResultDto
            {
                Operation = "MarkAsUnpaid",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
            {
                { "Settlement description", settlement.Description},
                { "User fist name", user.FirstName },
                { "User last name", user.LastName }
            }
            };

            return ServiceResponseFactory.Success(result, "Settlement was successfully unpaid");
        }
        catch (Exception ex)
        {
            return ServiceResponseFactory.Error<OperationResultDto>(
                null,
                $"Failed to change settlement status: {ex.Message}",
                HttpStatusCode.InternalServerError
            );
        }
    }
}
