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

public class SettlementService(DataContext _context) : ISettlementService
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


    public Task<ServiceResponse<OperationResultDto>> DeleteSettlement(string householdId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<OperationResultDto>> EditSettlement(string householdId, EditSettlementRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<List<SettlementResponse>>> GetAllSettlements(string householdId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<SettlementResponse>> GetSettlement(string settlementId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<OperationResultDto>> MarkAsCompleted(string settlementId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<OperationResultDto>> UnMarkAsCompleted(string settlementId)
    {
        throw new NotImplementedException();
    }
}
