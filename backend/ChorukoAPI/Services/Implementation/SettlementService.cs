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
using System.Net;

namespace ChorukoAPI.Services.Implementation;

public class SettlementService(
    DataContext _context,
    IMapper<Settlement, SettlementResponse> _mapper,
    IMapper<Settlement, SettlementWithoutUsersResponse> mapper) : ISettlementService
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

            var allParticipants = household.Participants ?? new List<User>();

            var requestedUnsettledIds = (request.UnsettledUsers == null || request.UnsettledUsers.Count == 0
                    ? allParticipants.Select(p => p.Id)
                    : request.UnsettledUsers)
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

            var settledUsers = allParticipants
                .Where(u => !requestedUnsettledIds.Contains(u.Id) && u.Id != userId)
                .ToList();

            var settlement = new Settlement
            {
                Id = Guid.NewGuid().ToString(),
                HouseholdId = householdId,
                CreatorId = userId,
                DateOfCreation = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                Description = request.Description,
                Amount = request.Amount,
                Status = SettlementStatus.Pending,
                UnsettledUsers = unsettledUsers,
                SettledUsers = settledUsers
            };

            _context.Settlements.Add(settlement);
            await _context.SaveChangesAsync();

            var result = new OperationResultDto
            {
                Operation = "CreateSettlement",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["SettlementId"] = settlement.Id,
                    ["UnsettledCount"] = unsettledUsers.Count,
                    ["SettledCount"] = settledUsers.Count
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

    public async Task<ServiceResponse<OperationResultDto>> EditSettlement(
    string settlementId,
    EditSettlementRequest request)
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

            var participants = settlement.Household?.Participants ?? new List<User>();

            var reqUnsettled = request.UnsettledUsers?.ToHashSet(StringComparer.Ordinal);
            var reqSettled = request.SettledUsers?.ToHashSet(StringComparer.Ordinal);

            if (reqUnsettled is not null && reqSettled is not null)
            {
                var overlap = reqUnsettled.Intersect(reqSettled).ToList();
                if (overlap.Count > 0)
                    return ServiceResponseFactory.Error<OperationResultDto>(null,
                        $"Users duplicated in both Unsettled and Settled: {string.Join(", ", overlap)}",
                        HttpStatusCode.BadRequest);
            }

            var participantIds = participants.Select(p => p.Id).ToHashSet(StringComparer.Ordinal);

            HashSet<string> targetUnsettled;
            HashSet<string> targetSettled;

            if (reqUnsettled is not null && reqSettled is not null)
            {
                targetUnsettled = reqUnsettled;
                targetSettled = reqSettled;
            }
            else if (reqUnsettled is not null)
            {
                targetUnsettled = reqUnsettled;
                targetSettled = participantIds.Except(targetUnsettled).ToHashSet(StringComparer.Ordinal);
            }
            else if (reqSettled is not null)
            {
                targetSettled = reqSettled;
                targetUnsettled = participantIds.Except(targetSettled).ToHashSet(StringComparer.Ordinal);
            }
            else
            {
                targetUnsettled = settlement.UnsettledUsers?.Select(u => u.Id).ToHashSet(StringComparer.Ordinal)
                                  ?? new HashSet<string>(StringComparer.Ordinal);
                targetSettled = settlement.SettledUsers?.Select(u => u.Id).ToHashSet(StringComparer.Ordinal)
                                  ?? new HashSet<string>(StringComparer.Ordinal);
            }

            var creatorId = settlement.CreatorId;
            targetUnsettled.Remove(creatorId);
            targetSettled.Remove(creatorId);

            var invalid = targetUnsettled.Concat(targetSettled).Where(id => !participantIds.Contains(id)).Distinct().ToList();
            if (invalid.Count > 0)
                return ServiceResponseFactory.Error<OperationResultDto>(null,
                    $"Some users are not participants of the household: {string.Join(", ", invalid)}",
                    HttpStatusCode.BadRequest);

            var newUnsettled = participants.Where(p => targetUnsettled.Contains(p.Id)).ToList();
            var newSettled = participants.Where(p => targetSettled.Contains(p.Id)).ToList();

            settlement.UnsettledUsers = newUnsettled;
            settlement.SettledUsers = newSettled;
            settlement.Description = request.Description;

            await _context.SaveChangesAsync();

            var op = new OperationResultDto
            {
                Operation = "EditSettlement",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["SettlementId"] = settlement.Id,
                    ["UnsettledCount"] = newUnsettled.Count,
                    ["SettledCount"] = newSettled.Count
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

    public async Task<ServiceResponse<OperationResultDto>> MarkAsCompleted(string settlementId)
    {
        try
        {
            var settlement = await _context.Settlements.FirstOrDefaultAsync(s => s.Id == settlementId);

            if (settlement == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "No settlement found", HttpStatusCode.NotFound);
            }

            settlement.Status = SettlementStatus.Completed;

            await _context.SaveChangesAsync();

            var result = new OperationResultDto
            {
                Operation = "MarkAsCompleted",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["SettlementId"] = settlement.Id
                }
            };

            return ServiceResponseFactory.Success(
                result,
                "Settlement marked as completed successfull",
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

    public async Task<ServiceResponse<OperationResultDto>> UnMarkAsCompleted(string settlementId)
    {
        try
        {
            var settlement = await _context.Settlements.FirstOrDefaultAsync(s => s.Id == settlementId);

            if (settlement == null)
            {
                return ServiceResponseFactory.Error<OperationResultDto>(null, "No settlement found", HttpStatusCode.NotFound);
            }

            settlement.Status = SettlementStatus.Pending;

            await _context.SaveChangesAsync();

            var result = new OperationResultDto
            {
                Operation = "UnMarkAsCompleted",
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
}
