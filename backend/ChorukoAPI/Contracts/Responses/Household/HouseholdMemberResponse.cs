namespace ChorukoAPI.Contracts.Responses.Household;

public record HouseholdParticipantResponse(
    string Id,
    string FirstName,
    string LastName,
    string? ProfilePicturePath,
    int pointsCurrentMonth,
    int pointsCurrentYear,
    int pointsAllTime
);
