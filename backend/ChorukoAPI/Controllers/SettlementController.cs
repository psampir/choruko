using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Settlement;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Household;
using ChorukoAPI.Extensions;
using ChorukoAPI.Mappers;
using ChorukoAPI.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChorukoAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SettlementController(ISettlementService settlementService) : ControllerBase
{

    [HttpGet("all/{householdId}")]
    [Authorize(Roles = "HouseholdAdmin,Participant")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> GetAllSettlement([FromRoute] string householdId)
    {

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<HouseholdResponse>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await settlementService.GetAllSettlements(householdId);

        return result.ToCreatedApiResponse();
    }

    [HttpGet("{settlementId}")]
    [Authorize(Roles = "HouseholdAdmin,Participant")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> GetSettlement([FromRoute] string settlementId)
    {

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<HouseholdResponse>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await settlementService.GetSettlement(settlementId);

        return result.ToCreatedApiResponse();
    }

    [HttpPost("create/{householdId}")]
    [Authorize(Roles = "HouseholdAdmin,Participant")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> CreateSettlement([FromRoute] string householdId, CreateSettlementRequest request)
    {

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<HouseholdResponse>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await settlementService.CreateSettlement(householdId, request, userId);

        return result.ToCreatedApiResponse();
    }

    [HttpPut("edit/{settlementId}")]
    [Authorize(Roles = "HouseholdAdmin,Participant")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> EditSettlement([FromRoute] string settlementId, EditSettlementRequest request)
    {

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<HouseholdResponse>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await settlementService.EditSettlement(settlementId, request);

        return result.ToCreatedApiResponse();
    }

    [HttpPut("markComplete/{settlementId}")]
    [Authorize(Roles = "HouseholdAdmin,Participant")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> MarkAsCompleted([FromRoute] string settlementId)
    {

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<HouseholdResponse>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await settlementService.MarkAsCompleted(settlementId);

        return result.ToCreatedApiResponse();
    }

    [HttpPut("unmarkComplete/{settlementId}")]
    [Authorize(Roles = "HouseholdAdmin,Participant")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> UnMarkAsCompleted([FromRoute] string settlementId)
    {

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<HouseholdResponse>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await settlementService.UnMarkAsCompleted(settlementId);

        return result.ToCreatedApiResponse();
    }

    [HttpDelete("delete/{settlementId}")]
    [Authorize(Roles = "HouseholdAdmin,Participant")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> DeleteSettlement([FromRoute] string settlementId)
    {

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<HouseholdResponse>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await settlementService.DeleteSettlement(settlementId);

        return result.ToCreatedApiResponse();
    }
}
