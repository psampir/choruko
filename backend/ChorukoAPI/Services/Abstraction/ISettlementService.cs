using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Settlement;
using ChorukoAPI.Contracts.Responses;

namespace ChorukoAPI.Services.Abstraction;

public interface ISettlementService
{
    Task<ServiceResponse<OperationResultDto>> CreateSettlement(string householdId, CreateSettlementRequest request);
    Task<ServiceResponse<OperationResultDto>> EditSettlement(string householdId, EditSettlementRequest request);
    Task<ServiceResponse<OperationResultDto>> MarkAsCompleted(string settlementId);
    Task<ServiceResponse<OperationResultDto>> UnMarkAsCompleted(string settlementId);
    Task<ServiceResponse<OperationResultDto>> DeleteSettlement(string householdId);
}
