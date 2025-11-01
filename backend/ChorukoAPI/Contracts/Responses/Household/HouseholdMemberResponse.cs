namespace ChorukoAPI.Contracts.Responses.Household;

public record HouseholdMemberResponse(
    string Id,
    string FirstName,
    string LastName,
    string? ProfilePicturePath,
    int PointsCurrentMonth, 
    int PointsCurrentYear,  
    int PointsAllTime);     
