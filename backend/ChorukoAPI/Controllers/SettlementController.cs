using ChorukoAPI.Contracts;
using ChorukoAPI.Contracts.Requests.Settlement;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Household;
using ChorukoAPI.Extensions;
using ChorukoAPI.Mappers;
using ChorukoAPI.Models;
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

        return result.ToCreatedApiResponse("Settlement list");
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

        return result.ToCreatedApiResponse("Settlement detail");
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

        return result.ToCreatedApiResponse("NewSettlement");
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

        return result.ToCreatedApiResponse("EditSettlement");
    }

    [HttpPut("pay/{settlementId}")]
    [Authorize(Roles = "HouseholdAdmin,Participant")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> SettlementPaid([FromRoute] string settlementId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<HouseholdResponse>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await settlementService.SettlementPaid(settlementId, userId);

        return result.ToCreatedApiResponse("PaySettlement");
    }

    [HttpPut("markDone/{settlementId}")]
    [Authorize(Roles = "HouseholdAdmin,Participant")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> MarkAsDone([FromRoute] string settlementId)
    {

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<HouseholdResponse>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await settlementService.MarkAsDone(settlementId);

        return result.ToCreatedApiResponse("Settlement complete");
    }

    [HttpPut("unmarkDone/{settlementId}")]
    [Authorize(Roles = "HouseholdAdmin,Participant")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> UnMarkAsDone([FromRoute] string settlementId)
    {

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<HouseholdResponse>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await settlementService.UnMarkAsDone(settlementId);

        return result.ToCreatedApiResponse("Settlement uncomplete");
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

        return result.ToCreatedApiResponse("Delete settlement");
    }
}
