using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Settlement;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Settlement;

namespace ChorukoAPI.Services.Abstraction;

public interface ISettlementService
{
    Task<ServiceResponse<List<SettlementWithoutUsersResponse>>> GetAllSettlements(string householdId);
    Task<ServiceResponse<SettlementResponse>> GetSettlement(string settlementId);
    Task<ServiceResponse<OperationResultDto>> CreateSettlement(string householdId, CreateSettlementRequest request, string userId);
    Task<ServiceResponse<OperationResultDto>> EditSettlement(string settlementId, EditSettlementRequest request, string userId);
    Task<ServiceResponse<OperationResultDto>> MarkAsPaid(string settlementId, string userId);
    Task<ServiceResponse<OperationResultDto>> MarAsUnPaid(string settlementId, string userId);
    Task<ServiceResponse<OperationResultDto>> MarkAsDone(string settlementId);
    Task<ServiceResponse<OperationResultDto>> UnMarkAsDone(string settlementId);
    Task<ServiceResponse<OperationResultDto>> DeleteSettlement(string settlementId);
}
