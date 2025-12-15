using ChorukoAPI.Contracts.Requests.Activity;
using ChorukoAPI.Models;

namespace ActivityService.Test.Helpers;

public static class TestData
{
    // Users
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
            Participants = participants ?? new List<User> { adminUser },
            Activities = new List<Activity>(),
            Tasks = new List<HouseholdTask>()
        };
    }

    // Activities
    public static Activity CreateActivity(
        string? id = null,
        string? name = null,
        Household? household = null,
        int pointsAwarded = 10)
    {
        var householdData = household ?? CreateHousehold();
        return new Activity
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = name ?? "Test Activity",
            HouseholdId = householdData.Id,
            Household = householdData,
            PointsAwarded = pointsAwarded
        };
    }

    // Request DTOs
    public static CreateActivityRequest CreateActivityRequest(
        string? name = null,
        int pointsAwarded = 10)
    {
        return new CreateActivityRequest
        {
            Name = name ?? "Test Activity",
            PointsAwarded = pointsAwarded
        };
    }

    public static EditActivityRequest EditActivityRequest(
        string? name = null,
        int pointsAwarded = 15)
    {
        return new EditActivityRequest
        {
            Name = name ?? "Edited Activity",
            PointsAwarded = pointsAwarded
        };
    }
}
