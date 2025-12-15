using ChorukoAPI.Contracts.Requests.Settlement;
using ChorukoAPI.Contracts.Responses.Settlement;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Models;
using ChorukoAPI.Contracts.Responses.User;

namespace SettlementService.Test.Helpers;

public static class TestData
{
    // User
    public static User CreateUser(
        string? id = null,
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        string? householdId = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Email = email ?? "user@test.com",
            FirstName = firstName ?? "John",
            LastName = lastName ?? "Doe",
            HouseholdId = householdId,
            PointsCurrentMonth = 0,
            PointsCurrentYear = 0,
            PointsAllTime = 0
        };
    }

    public static User CreateAdmin(string? id = null)
    {
        return CreateUser(
            id: id ?? "admin-1",
            email: "admin@test.com",
            firstName: "Admin",
            lastName: "User"
        );
    }

    // Households
    public static Household CreateHousehold(
        string? id = null,
        string? name = null,
        User? admin = null,
        string? currencyCode = null,
        List<User>? participants = null)
    {
        var adminUser = admin ?? CreateAdmin();
        return new Household
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = name ?? "Test Household",
            AdminId = adminUser.Id,
            Admin = adminUser,
            CurrencyCode = currencyCode ?? "USD",
            Participants = participants ?? new List<User> { adminUser }
        };
    }

    // Settlements
    public static Settlement CreateSettlement(
        string? id = null,
        string? description = null,
        decimal amount = 100.0m,
        Household? household = null,
        User? creator = null,
        List<User>? unsettledUsers = null,
        List<User>? settledUsers = null,
        SettlementStatus? status = null)
    {
        var householdData = household ?? CreateHousehold();
        var creatorUser = creator ?? CreateAdmin();

        return new Settlement
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Description = description ?? "Test Settlement",
            Amount = amount,
            HouseholdId = householdData.Id,
            Household = householdData,
            CreatorId = creatorUser.Id,
            Creator = creatorUser,
            UnsettledUsers = unsettledUsers ?? new List<User>(),
            SettledUsers = settledUsers ?? new List<User>(),
            Status = status ?? SettlementStatus.Pending,
            DateOfCreation = DateTime.UtcNow
        };
    }

    // Request DTOs for Settlement
    public static CreateSettlementRequest CreateSettlementRequest(
        string? description = null,
        decimal amount = 100.0m,
        List<string>? unsettledUsers = null)
    {
        return new CreateSettlementRequest
        {
            Description = description ?? "Test Settlement",
            Amount = amount,
            UnsettledUsers = unsettledUsers ?? new List<string>()
        };
    }

    public static EditSettlementRequest EditSettlementRequest(
        string? description = null,
        decimal amount = 150.0m,
        List<string>? unsettledUsers = null)
    {
        return new EditSettlementRequest
        {
            Description = description ?? "Edited Settlement",
            Amount = amount,
            UnsettledUsers = unsettledUsers ?? new List<string>()
        };
    }

    // Response DTOs
    public static SettlementResponse CreateSettlementResponse(
        string? id = null,
        string? householdId = null,
        string? description = null,
        decimal amount = 100.0m,
        SettlementStatus? status = null,
        string? creatorId = null,
        UserInfoResponse? creator = null,
        DateTime? dateOfCreation = null,
        List<UserInfoResponse>? settledUsers = null,
        List<UserInfoResponse>? unsettledUsers = null)
    {
        return new SettlementResponse
        {
            Id = id ?? Guid.NewGuid().ToString(),
            HouseholdId = householdId ?? Guid.NewGuid().ToString(),
            CreatorId = creatorId ?? Guid.NewGuid().ToString(),
            Creator = creator ?? CreateUserInfoResponse(
                firstName: "Creator",
                lastName: "User"
            ),
            DateOfCreation = dateOfCreation ?? DateTime.UtcNow,
            Description = description ?? "Test Settlement",
            Amount = amount,
            Status = status ?? SettlementStatus.Pending,
            SettledUsers = settledUsers,
            UnsettledUsers = unsettledUsers
        };
    }

    // Response DTOs
    public static SettlementWithoutUsersResponse CreateSettlementWithoutUsersResponse(
        string? id = null,
        string? householdId = null,
        string? description = null,
        decimal amount = 100.0m,
        SettlementStatus? status = null,
        string? creatorId = null,
        UserInfoResponse? creator = null,
        DateTime? dateOfCreation = null,
        List<string>? settledUserIds = null,
        List<string>? unsettledUserIds = null)
    {
        return new SettlementWithoutUsersResponse
        {
            Id = id ?? Guid.NewGuid().ToString(),
            HouseholdId = householdId ?? Guid.NewGuid().ToString(),
            CreatorId = creatorId ?? Guid.NewGuid().ToString(),
            Creator = creator ?? CreateUserInfoResponse(
                firstName: "Creator",
                lastName: "User"
            ),
            DateOfCreation = dateOfCreation ?? DateTime.UtcNow,
            Description = description ?? "Test Settlement",
            Amount = amount,
            Status = status ?? SettlementStatus.Pending,
            SettledUserIds = settledUserIds,
            UnsettledUserIds = unsettledUserIds
        };
    }


    public static UserInfoResponse CreateUserInfoResponse(
        string? id = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? householdId = null,
        string? profilePicturePath = null,
        int pointsCurrentMonth = 0,
        int pointsCurrentYear = 0,
        int pointsAllTime = 0)
    {
        return new UserInfoResponse(
            id ?? Guid.NewGuid().ToString(),
            firstName ?? "John",
            lastName ?? "Doe",
            email ?? "user@test.com",
            householdId,
            profilePicturePath,
            pointsCurrentMonth,
            pointsCurrentYear,
            pointsAllTime
        );
    }
}
