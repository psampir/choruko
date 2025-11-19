using System.Collections.Generic; 

namespace ChorukoAPI.Contracts.Responses.Household;

public class HouseholdResponse
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string AdminId { get; set; }
    public required string AdminFirstName { get; set; }
    public required string AdminLastName { get; set; }
    public int ParticipantsCount { get; set; }
    public string? ProfilePictureUrl { get; set; }
}