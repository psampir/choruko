namespace ChorukoAPI.Contracts.Responses.Invitation;

public class InvitationResponse
{
    public required string Id { get; set; }

    public required string ReceiverId { get; set; }

    public required string HouseholdName { get; set; }

    public required string HouseholdImagePath { get; set; }
}
