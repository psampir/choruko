using System.Collections.Generic; 

namespace ChorukoAPI.Contracts.Responses.Household;

public class HouseholdResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string AdminId { get; set; }
    public string AdminFirstName { get; set; }
    public string AdminLastName { get; set; }
    public int ParticipantsCount { get; set; }
    public string? ProfilePictureUrl { get; set; }
}