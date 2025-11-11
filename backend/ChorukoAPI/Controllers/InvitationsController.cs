using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts;
using ChorukoAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ChorukoAPI.Services.Abstraction;
using ChorukoAPI.Mappers;
using ChorukoAPI.Contracts.Responses.Invitation;

namespace ChorukoAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InvitationsController(IInvitationService invitationService) : ControllerBase
{
    [HttpPost("{householdId}/create")]
    [Authorize(Roles = "HouseholdAdmin")]
    public async Task<ActionResult<ApiResponse<OperationResultDto>>> InviteUserToHousehold([FromRoute] string householdId, string userIdToAdd)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<OperationResultDto>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await invitationService.CreateHouseholdInvitation(userId, householdId, userIdToAdd);

        return result.ToApiResponse("Invite participant)");
    }

    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<List<InvitationResponse>>>> GetAllInvitations()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<OperationResultDto>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await invitationService.GetAllInvitations(userId);

        return result.ToApiResponse("Invite participant)");
    }

    [HttpGet("{invitationId}")]
    public async Task<ActionResult<ApiResponse<List<InvitationResponse>>>> GetAllInvitations([FromRoute] string invitationId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ServiceResponseFactory.Error<OperationResultDto>(null, "User not authenticated.").ToApiResponse());
        }

        var result = await invitationService.GetInvitation(invitationId);

        return result.ToApiResponse("Invite participant)");
    }
}
