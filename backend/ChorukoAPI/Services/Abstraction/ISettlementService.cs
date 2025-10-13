using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Settlement;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Settlement;

namespace ChorukoAPI.Services.Abstraction;

public interface ISettlementService
{
    Task<ServiceResponse<List<SettlementResponse>>> GetAllSettlements(string householdId);
    Task<ServiceResponse<SettlementResponse>> GetSettlement(string settlementId);
    Task<ServiceResponse<OperationResultDto>> CreateSettlement(string householdId, CreateSettlementRequest request, string userId);
    Task<ServiceResponse<OperationResultDto>> EditSettlement(string settlementId, EditSettlementRequest request);
    Task<ServiceResponse<OperationResultDto>> MarkAsCompleted(string settlementId);
    Task<ServiceResponse<OperationResultDto>> UnMarkAsCompleted(string settlementId);
    Task<ServiceResponse<OperationResultDto>> DeleteSettlement(string settlementId);
}
