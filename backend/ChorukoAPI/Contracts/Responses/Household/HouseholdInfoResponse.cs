namespace ChorukoAPI.Contracts.Responses.Household;

public record HouseholdInfoResponse(
    string Id,
    string Name,
    string AdminId,
    string AdminFirstName,
    string AdminLastName,
    int ParticipantsCount,
    string? ProfilePictureUrl,
    string CurrencySymbol,
    string ISOCurrencySymbol,
    List<HouseholdParticipantResponse> Participants);
