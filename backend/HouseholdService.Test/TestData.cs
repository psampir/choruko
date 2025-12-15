using ChorukoAPI.Models;
using ChorukoAPI.Contracts.Requests.Household;
using ChorukoAPI.Contracts.Responses;
using ChorukoAPI.Contracts.Responses.Household;
using Microsoft.AspNetCore.Http;
using ChorukoAPI.Contracts.Responses.Activity;
using ChorukoAPI.Contracts.Responses.User;
using System.Xml.Linq;

namespace HouseholdService.Test.Helpers;

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

    public static User CreateAdmin(string? id = null, string? householdId = null)
    {
        return CreateUser(
            id: id ?? "admin-1",
            email: "admin@test.com",
            firstName: "Admin",
            lastName: "User",
            householdId: householdId
        );
    }

    // Households
    public static Household CreateHousehold(
        string? id = null,
        string? name = null,
        User? admin = null,
        string? currencyCode = null,
        List<User>? participants = null,
        string? profilePicturePath = null)
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
            Tasks = new List<HouseholdTask>(),
            ProfilePicturePath = profilePicturePath
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

    // Request DTOs for Household
    public static CreateHouseholdRequest CreateHouseholdRequest(
        string? name = null,
        string? currencyCode = null,
        IFormFile? profilePicture = null)
    {
        return new CreateHouseholdRequest
        {
            Name = name ?? "Test Household",
            CurrencyCode = currencyCode ?? "USD",
            ProfilePicture = profilePicture
        };
    }

    public static ChangeHouseholdPictureRequest ChangeHouseholdPictureRequest(
        IFormFile? profilePicture = null)
    {
        return new ChangeHouseholdPictureRequest
        {
            ProfilePicture = profilePicture
        };
    }

    public static ChangeCurrencyRequest ChangeCurrencyRequest(string? newCurrencyCode = null)
    {
        return new ChangeCurrencyRequest
        {
            NewCurrencyCode = newCurrencyCode ?? "EUR"
        };
    }

    // Response DTOs
    public static HouseholdInfoResponse CreateHouseholdInfoResponse(
        string? id = null,
        string? name = null,
        string? adminId = null,
        string? adminFirstName = null,
        string? adminLastName = null,
        int participantsCount = 1,
        string? profilePictureUrl = null,
        string? currencySymbol = null,
        string? isoCurrencySymbol = null,
        List<HouseholdParticipantResponse>? participants = null)
    {
        return new HouseholdInfoResponse(
            Id: id ?? Guid.NewGuid().ToString(),
            Name: name ?? "Test Household",
            AdminId: adminId ?? Guid.NewGuid().ToString(),
            AdminFirstName: adminFirstName ?? "Admin",
            AdminLastName: adminLastName ?? "User",
            ParticipantsCount: participantsCount,
            ProfilePictureUrl: profilePictureUrl,
            CurrencySymbol: currencySymbol ?? "$",
            ISOCurrencySymbol: isoCurrencySymbol ?? "USD",
            Participants: participants ?? new List<HouseholdParticipantResponse>()
        );
    }

    public static HouseholdParticipantResponse CreateHouseholdParticipantResponse(
        string? id = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? profilePicturePath = null,
        int pointsCurrentMonth = 0,
        int pointsCurrentYear = 0,
        int pointsAllTime = 0)
    {
        return new HouseholdParticipantResponse(
            Id: id ?? Guid.NewGuid().ToString(),
            FirstName: firstName ?? "John",
            LastName: lastName ?? "Doe",
            ProfilePicturePath: profilePicturePath,
            PointsCurrentMonth: pointsCurrentMonth,
            PointsCurrentYear: pointsCurrentYear,
            PointsAllTime: pointsAllTime
        );
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

    public static ActivityResponse CreateActivityResponse(
        string? id = null,
        string? name = null,
        int pointsAwarded = 10)
    {
        return new ActivityResponse
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = name ?? "Test Activity",
            PointsAwarded = pointsAwarded
        };
    }
}
