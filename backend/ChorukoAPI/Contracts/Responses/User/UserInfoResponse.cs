namespace ChorukoAPI.Contracts.Responses.User;

public record UserInfoResponse(
    string Id,
    string FirstName, 
    string LastName, 
    string Email,
    string? HouseholdId,
    string? ProfilePicturePath,
    int PointsCurrentMonth,
    int PointsCurrentYear,
    int PointsAllTime
    );