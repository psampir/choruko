namespace ChorukoAPI.Contracts.Responses.Household;

public record HouseholdParticipantResponse(
    string Id,
    string FirstName,
    string LastName,
    string? ProfilePicturePath,
    int PointsCurrentMonth, 
    int PointsCurrentYear,  
    int PointsAllTime);     
