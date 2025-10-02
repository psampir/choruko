using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Settlement;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Settlement;
using ChorukoAPI.Services.Abstraction;

namespace ChorukoAPI.Services.Implementation;

public class SettlementService : ISettlementService
{
    public Task<ServiceResponse<OperationResultDto>> CreateSettlement(string householdId, CreateSettlementRequest request)
    {
        throw new NotImplementedException();
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
